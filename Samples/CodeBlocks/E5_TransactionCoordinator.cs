using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Perigee;
using Perigee.Extensions;
using Perigee.FileFormats.CSV;
using Perigee.Integration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{

    /*
        ---- Transaction Coordinator ----
        -- https://docs.perigee.software/core-modules/integration-utilities/transaction-coordinator

        Transaction Coordinator demonstrates how to set up and manage a multi-step transaction process within your application, ensuring reliability and fault-tolerance.
        
        The coordinator is extremely fault-tolerant: 
            * Try sending duplicate order IDs and watch it deduplicate them.
            * Try setting a breakpoint and stopping the application in the middle of one of the three steps, then watch the application resume.
            * Try turning your network off and on, watch the MultiStepOptions.RequireInternet option respond by shutting down the processor
            * Try to replay an order by sending a replay command.

        --== Learning Objective #1: Understanding the Coordinator Concept ==--
        * Learn how the Coordinator operates like a group of small robots performing specific tasks.
        * Understand the fault-tolerant nature of the system, where data is saved and processes can restart without loss.
        * Observe how the Coordinator functions like a message queue, processing messages in a first-in-first-out manner (multi-threaded).

        --== Learning Objective #2: Initial Setup of Transaction Coordinator ==--
        * Learn how to initialize a transaction coordinator to manage complex multi-step processes.
        * Understand the importance of using a transaction source, such as a database or memory source, for storing transaction data.
        * Observe the setup of a transaction coordinator with a memory source for demo purposes.

        --== Learning Objective #3: Configuring Multi-step Processes ==--
        * Learn how to configure a multi-step process within the transaction coordinator.
        * Understand the sequence of executing multiple steps, such as ordering, reporting, and generating CSVs.
        * Observe the execution flow and logging of each step within the multi-step process.

        --== Learning Objective #4: Remote Synchronization and Logging ==--
        * Learn how the coordinator handles two-way data synchronization with a remote source.
        * Understand how each bot's state is replicated to the remote source, allowing the transaction to resume and provide detailed logging.
        * Observe how the coordinator ensures that each step is replicated and stored on the remote server.

        --== Learning Objective #5: Queueing and Re-queueing Process ==--
        * Understand the queueing process where items are added and processed in order.
        * Learn about the automatic re-queueing of incomplete transactions and the handling of retries.
        * Observe how the coordinator pulls pending transactions from the remote source and manages their states.
        * Try setting a breakpoint or throwing an exception in the middle of one of the tasks

        --== Learning Objective #6: Replay Events ==--
        * Understand the replay functionality for transactions, allowing the re-execution of processes.
        * Learn how to use transaction IDs to replay transactions from the remote source.
        * Observe the coordinator's ability to run through all the logic for a given transaction again at a later date.

        --== Learning Objective #7: Practical Application of Transaction Coordination ==--
        * Learn why using a transaction coordinator can enhance task coordination and reliability in real-world applications.
        * Understand the practical benefits of using a transaction coordinator for managing complex task dependencies.
        * Observe how the transaction coordinator ensures data validity and task synchronization.

        --> To run this sample:
        E5_TransactionCoordinator.run();
    */
    public static class E5_TransactionCoordinator
    {
        public static void run()
        {

            //To call order, use postman or curl:
            //curl https://localhost:7216/queue?number=66

            //To call replay (to re-process a transaction), use postman or curl:
            //curl https://localhost:7216/replay?number=66

            PerigeeApplication.ApplicationNoInit("Coordinator", (c) =>
            {
                //Local source, which in a real-world scenario, you'd likely be using a database source
                var localSource = new ITransactionSource_Memory("ITSourceMemory.json", c.GetCancellationToken());

                //To clear local directory on load (for ease of testing)
                //      if (Directory.Exists($"TC{Path.DirectorySeparatorChar}OrderProcess")) Directory.Delete($"TC{Path.DirectorySeparatorChar}OrderProcess", true);
                //      if (File.Exists("ITSourceMemory.json")) File.Delete("ITSourceMemory.json");


                //Add a coordinator named "OrderProcess" to the system
                c.AddTransactionCoordinator("OrderProcess", localSource, (ct, l, process) =>
                {
                    using var client = new RestClient("https://localhost:7216");

                    //MultiStep process that executes the following blocks in order: order, report, csv
                    process.MultiStep(new string[] { "order", "report", "csv" }, MultiStepOptions.RequireInternet,
                        (order) =>
                        {
                            //Get the initial data object, a string
                            var orderString = $"O-{order.GetInitialDataObjectAs<string>()}";

                            if (order.IsReplay)
                                l.LogInformation("Replaying order {order}", orderString);
                            else
                            l.LogInformation("Executing order {order}", orderString);

                            var rsp = order.Execute<OrderResponse>(client, new RestRequest("/order", Method.Get).AddParameter("order", orderString));
                        },
                        (report) =>
                        {
                            //Get the data object from the last step, an OrderResponse
                            var order = report.GetDataObjectFromLastStepAs<OrderResponse>();
                            l.LogInformation("Reporting order {@order} to record service", order);

                            var rsp = report.Execute(client, new RestRequest("/report", Method.Get).AddParameter("order", order.Order).AddParameter("created", order.CreatedAt.ToString("O")));
                        },
                        (csv) =>
                        {
                            //Get the response body, since we're logging it.
                            var previousResponseBody = csv.GetPreviousItem()!.ResponseBody;

                            //Get the original item, "order".
                            var orderItem = csv.GetItemWithName("order")!;
                            l.LogInformation("Order has been completed! Response was: {body}", previousResponseBody);

                            //Generate a CSV and save it to our filesystem
                            var DT = new List<OrderResponse>() { JsonConvert.DeserializeObject<OrderResponse>(previousResponseBody)! }.ToDataTable();
                            new CSVWriter(DT).WriteFile($"OrderCSV{Path.DirectorySeparatorChar}Order_{orderItem.TransactionID}.{orderItem.ReplayID}.csv");
                            l.LogInformation("Complete! Wrote CSV to path");

                            //As this isn't using .Execute, we need to set the status to complete the item, and header. 
                            csv.SetStatus(TransactionStatus.Completed);
                        }
                    );
                }, LocalPullTimespan: TimeSpan.FromSeconds(15), RemotePullTimespan: TimeSpan.FromSeconds(60));


                //Add an API to trigger the processes
                c.AddMinimalAPI("DemoAPI", 7216, (r) =>
                {

                    //To queue a new order with number
                    r.MapGet("/queue", ([FromQuery] string number, TransactionCoordinator tc) =>
                    {
                        tc.QueueTransaction(TransactionHeader.Transaction(number, number));
                        return Results.Ok("Enqueued");
                    });

                    //Replay an order
                    r.MapGet("/replay", ([FromQuery] string number, TransactionCoordinator tc) =>
                    {
                        try
                        {
                            tc.ReplayTransaction(number);
                            return Results.Ok("Replay started");
                        }
                        catch (Exception)
                        {
                            return Results.BadRequest("Couldn't replay that ID");
                        }
                    });

                    //An API method to get an order and respond with it
                    r.MapGet("/order", ([FromQuery] string order) => Results.Json(new { Order = order, CreatedAt = DateTimeOffset.Now }, statusCode: 201));
                    r.MapGet("/report", ([FromQuery] string order, [FromQuery] DateTimeOffset created) => Results.Json(new { Order = order, CreatedAt = created, Status = "order successfully reported to record system" }, statusCode: 200));

                },

                //This line adds the singleton for this transaction coordinator to the DI pipeline for the AspNetCore.http calls.
                // It enables the `TransactionCoordinator tc` to be injected into the http calls
                (b, s) => { s.AddSingleton(c.GetTransactionCoordinator("OrderProcess")!); });

            });

        }
    }

    public class OrderResponse { 
        public string Order { get; set; } 
        public DateTimeOffset CreatedAt { get; set; } 
    }
}
