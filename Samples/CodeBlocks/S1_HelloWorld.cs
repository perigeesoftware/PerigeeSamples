using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;

namespace Samples.CodeBlocks
{
    /*
    ---- Hello World ----
    - https://docs.perigee.software/

    This sample demonstrates the basics of running a Perigee application.
    It pulls in configuration settings, sets up an ecosystem for graceful shutdown,
    parallel task execution, and provides a fully programmatic interface into the running code.

    It configures the Perigee start, and begins two tasks:
        - A recurring task ("Hello Perigee") that logs an informational message every configurable interval.
        - A CRON-scheduled background processor that runs every 5 seconds and must complete its task before shutdown.

    --== Learning Objective #1: Graceful Shutdown ==--
    * Start the application, then press CTRL-C on the keyboard.
        Watch as Perigee shuts down gracefully while ensuring that any running tasks complete their work.

    --== Learning Objective #2: Dynamic Configuration ==--
    * Note how the recurring task interval is dynamically pulled from configuration ("DelaySeconds").
    * Modify the "DelaySeconds" setting and re-run. Notice the recurring interval updates accordingly.
    

    --== Learning Objective #3: Thread Restart Control ==--
    * Look at the restart function, understand how this can grant great control over when to restart a crashed thread.
    * Understand how this could be used for a full reset before starting again, or monitoring for favorable conditions (like a network active, a database reachable, etc)
    * Uncomment the `config.GetThread("Sir Crash A Lot").QueueStop(true);` line below, play with the true|false and understand how it skips the restart function

    --> To run this sample:
    S1_HelloWorld.run();
*/

    public static class S1_HelloWorld
    {
        public static void run()
        {
            bool Throw = true;

            PerigeeApplication.App("Hello World!", (config) =>
            {
                // 1) Schedules a recurring task that logs an informational message every configurable interval.
                config.AddRecurring("Hello Perigee", (cancellationToken, logger) =>
                {
                    // Log a message to indicate the application is active. The message also instructs the user to press CTRL-C for a graceful shutdown.

                    // "DelaySeconds" is pulled from configuration to dynamically set the interval.
                    logger.LogInformation("I'm saying hello world every {n} seconds! Press Ctrl-C to initiate a graceful shutdown.", config.GetAppSetting<int>("DelaySeconds"));

                }, TimeSpan.FromSeconds(config.GetAppSetting<int>("DelaySeconds")));



                // 2) Schedules the background processor to run every 5 seconds using CRON syntax.
                //      This asynchronous task MUST complete its work before shutting down when CTRL-C is pressed.
                config.AddCRONAsync("Background Processor", "*/5 * * * * *", async (cancellationToken, logger) =>
                {
                    logger.LogInformation("I'm starting a very important background task...");

                    // Register a callback to log a warning if shutdown is requested during the critical operation.
                    var registration = cancellationToken.Register(() =>
                        logger.LogWarning("I know you requested shutdown, but I'm currently running. Please wait a moment and I'll exit.")
                    );

                    // Simulate critical work that takes 15 seconds.
                    await Task.Delay(TimeSpan.FromSeconds(15));

                    // Unregister the shutdown warning callback and log that the background task is complete.
                    registration.Unregister();
                    logger.LogInformation("Background task done. We may exit if requested.");
                });


                // 3) Let's schedule a task to simulate something that regularly has issues
                config.AddRecurring("Sir Crash A Lot", (ct, l) => {

                    l.LogInformation("Simulating refresh");
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();

                    //Crash!
                    if (Throw)
                        throw new Exception("Crash time");

                    l.LogInformation("Refresh done!");

                }, restartingWatch: () => {

                    //Check and make sure the network is good
                    if (!NetworkUtility.Available())
                        return false;

                    //Check a database? Execute a refresh/reset on an API? Do anything you like here!
                    // Let's fix the throwing issue. Simulating conditions favorable to run again.
                    Throw = false;

                    //True means we can restart the downed thread
                    return true;
                });

                //config.GetThread("Sir Crash A Lot").QueueStop(true);

                

            });

        }
    }
}
