using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;

namespace Samples.CodeBlocks
{

    /*
    ---- Task Control ----

    Task Control demonstrates how you can use the various thread types to achieve all kinds of fine-grain control over your threads

    --== Learning Objective #1: Adding CRON tasks ==--
    * Understand how to set up and configure a CRON task
    * Learn how to schedule a task to run at regular intervals (every 15 seconds in this example)
    * Observe the task running and view the output in the logs, try changing the CRON to run at different intervals!

    --== Learning Objective #2: Long-running tasks ==--
    * Learn how to create a task that runs indefinitely until explicitly stopped
    * Implement a loop that periodically checks for cancellation
    * Control the task via a minimal API endpoint using curl commands

    --== Learning Objective #3: Network-dependent tasks ==--
    * Understand how to create tasks that are dependent on network availability
    * Learn how to use a wrapper that stops the task when the network is unavailable
    * Turn the network off or on and see the thread react

    --== Learning Objective #4: Task control via API ==--
    * Set up a minimal API endpoint to control tasks
    * Use curl commands to start and stop tasks dynamically
    * Understand how to check for task existence and control their execution state

    --== Learning Objective #5: Recurring task management ==--
    * Learn how to configure recurring tasks that restart all running threads at specified intervals
    * Understand the importance of maintaining application health and stability by periodically restarting tasks
    * Observe and verify the behavior of the janitor thread in the application logs

    --> To run this sample:
    T1_Control.run();
    */
    public static class T1_Control
    {
        public static void run()
        {

            PerigeeApplication.ApplicationNoInit("FirstApp", (c) =>
            {

                //Cron method, running every 15 seconds
                c.AddCRON("HelloWorld", "*/15 * * * * *", (ct, l) =>
                {
                    l.LogInformation("Hello From {appName}! You can turn this thread on and off using: curl \"https://localhost:7222/task?task=HelloWorld&start=false\"", c.AppName);
                });

                //Running task that won't exit until requested
                c.Add("RunningTask", (ct, l) =>
                {

                    //Standardized long running task
                    l.LogInformation("Running Task starting and won't exit until requested");
                    l.LogInformation("You can turn this thread on and off using: curl \"https://localhost:7222/task?task=RunningTask&start=false\"");

                    //This is a method that is not expected to exit, so enter a low thread loop, and await cancellation or end
                    while (PerigeeApplication.delayOrCancel(10000, ct)) { }

                });

                //A network condition thread, that will stop executing when the network goes down
                c.Add("NetworkCondition", (parentToken, l) =>
                {
                    ThreadCondition.Wrap(parentToken,
                        (childToken) => T1_NetworkTask.Start(childToken, l),
                        () => NetworkUtility.Available(),
                        TimeSpan.FromSeconds(5));
                });


                //Add a task control endpoint
                // curl "https://localhost:7222/task?task=RunningTask&start=true"
                c.AddMinimalAPI("MAPI", 7222, (r) =>
                {
                    r.MapGet("/task", ([FromQuery] string task, [FromQuery] bool start) =>
                    {
                        if (c.ContainsThreadByName(task))
                            if (start) c.StartIfNotRunning(task);
                            else c.QueueStop(task);

                        return Results.Ok();
                    });

                });

                //The janitor thread will restart every running thread every <TimeSpan> supplied
                c.AddRecurring("Janitor", (ct, l) => c.RestartAllThreads(), (int)TimeSpan.FromHours(6).TotalMilliseconds);

            });
        }
    }

    /// <summary>
    /// This example class is a static method demonstrating how to write a task that gracefully shuts off when the network goes down.
    /// </summary>
    public class T1_NetworkTask
    {
        public static async Task Start(CancellationToken cancellationToken, ILogger l)
        {
            l.LogInformation("Starting my task...");
            while (await PerigeeApplication.delayOrCancelAsync(5000, cancellationToken))
            {
                l.LogInformation("My Task is running");
            }
            l.LogInformation("Ending my task...");
        }
    }
}
