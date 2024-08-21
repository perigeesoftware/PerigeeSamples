using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
    /*
        ---- Multi-Threaded Processor ----

        Multi-Threaded Processor demonstrates how to efficiently process data in parallel across multiple threads, using both simple and advanced approaches for scatter-gather operations.

        --== Learning Objective #1: Initial Setup of Multi-Threaded Processing ==--
        * Learn how to initialize and set up a multi-threaded processor within an application.
        * Understand the concept of scatter-gather processing and how it can be applied to parallelize tasks.
        * Observe the setup of a multi-threaded processing example with a list of fruits.

        --== Learning Objective #2: Basic Multi-Threaded Processing with IEnumerable ==--
        * Learn how to use the IEnumerable extension method for basic scatter-gather processing across multiple threads.
        * Understand how to configure and control the level of concurrency by specifying the number of threads.
        * Observe the processing of data in parallel and the collection of results, including exceptions and processing times.

        --== Learning Objective #3: Advanced Multi-Threaded Processing with Custom Processors ==--
        * Learn how to set up and use a custom MultiThreadedProcessor for more complex processing scenarios.
        * Understand how to separate the loading and processing phases across threads, providing more control over the operation.
        * Observe how to attach events to monitor and log the progress of data as it is processed across multiple threads.

        --== Learning Objective #4: Practical Application of Multi-Threaded Processing ==--
        * Learn why using a multi-threaded processor can significantly enhance the performance and efficiency of data processing in real-world applications.
        * Understand the practical benefits of using multi-threaded processing for handling large datasets or time-consuming tasks.
        * Observe how multi-threaded processing ensures controlled and efficient parallel data processing.

        --> To run this sample:
        U6_MultiThreadedProcessor.run();
    */

    public static class U6_MultiThreadedProcessor
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("MTP", (c) => {

                c.Add("Multi-Threaded Processor", (ct, l) => {

                    //Let's declare a list of fruits to work on
                    var Fruits = new List<string>()
                    {
                        "Apples",
                        "Bananas",
                        "Cherries",
                        "Dates",
                        "Elderberries",
                        "Figs",
                        "Grapes",
                        "Honeydew",
                        "Indian Fig",
                        "Jackfruit",
                        "Kiwi",
                        "Lemon",
                        "Mango",
                        "Nectarine",
                        "Oranges",
                        "Papaya",
                        "Quince",
                        "Raspberries",
                        "Strawberries",
                        "Tangerines",
                        "Ugli Fruit",
                        "Vanilla Bean",
                        "Watermelon",
                        "Xigua",
                        "Yellow Passion Fruit"
                    };

                    l.LogInformation("--------============ Example 1 ============--------");
                    //Example 1: Using the IEnumerable extension method to simple "scatter gather" across a number of defined threads. In this case, 5 threads are used
                    var mtpResult = Fruits.ParallelProcessMultiThread((s) =>
                    {
                        l.LogInformation("Processing {fruit}", s);
                        Task.Delay(400).Wait(); //Artificial delay
                        return s.GetHashCode();
                    }, null, concurrency: 5);

                    //Every object in this case is wrapped in a result object, giving you exceptions thrown, process time, and access to the input/output
                    foreach (var rs in mtpResult)
                    {
                        l.LogInformation("Passed in {in}, got {out}, in {time}ms", rs.InData, rs.OutData, rs.ProcessTime?.TotalMilliseconds.ToString("N0") ?? "");
                    }

                    //Example 2: Separate the loading and processing of the scatter gather across threads
                    //  In this example, we're declaring the multi-threaded processor directly, along with the input type, output type, and the method to be called.
                    //  Notice the 3 on the ThreadCount? This will only process and declare 3 threads to work on input items
                    l.LogInformation("--------============ Example 2 ============--------");
                    var mtp = new MultiThreadedProcessor<string, int>((s) =>
                    {
                        l.LogInformation("Processing {fruit}", s);
                        return s.GetHashCode();
                    }, ct, ThreadCount: 3, l);

                    //To actively watch for processed items, attach to the event
                    mtp.OnDataProcessed += (sender, args) =>
                    {
                        l.LogInformation("Passed in {in}, got {out}, in {time}ms", args.InData, args.OutData, args.ProcessTime?.TotalMilliseconds.ToString("N0") ?? "");
                    };

                    //Now let's simulate a delayed loading of the items by using Enqueue
                    foreach (var fruit in Fruits)
                    {
                        mtp.Enqueue(fruit);
                        Task.Delay(Random.Shared.Next(50, 200)).Wait(); //Artificial random delay
                    }

                    //Once all of the items have been enqueued, wait for the last item to be processed
                    mtp.AwaitProcessed(ct);


                    //Done! Everything has been processed and we have successfully awaited a multi-threaded, separated "scatter gather"


                    while (PerigeeApplication.delayOrCancel(1000, ct)) { };

                });
            
            });
        }
    }
}
