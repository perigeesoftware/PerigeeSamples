using Microsoft.Extensions.Logging;
using Perigee;

namespace Samples.CodeBlocks
{
    /*
    ---- Hello world ----
    - https://docs.perigee.software/

    Hello world demonstrates the basics of running a perigee application. 
     It configures the perigee start, and begins a recurring thread to log hello every 5 seconds.

    --== Learning Objective #1: Graceful Shutdown ==--
    * Start the application, then press CTRL-C on the keyboard. Watch as perigee shuts down the process while respecting the currently running threads

    --== Learning Objective #2: Change the recurring time ==--
    * Go the S1_HelloWorld.cs file
    * At the end of the .AddRecurring line, change the value from 5000 to 10000 (milliseconds) and re-run. Notice the recurring time is now 10 seconds?
    

    --> To run this sample:
    S1_HelloWorld.run();
    */
    public static class S1_HelloWorld
    {
        public static void run()
        {

            PerigeeApplication.ApplicationNoInit("Hello World!", (c) =>
            {
                c.AddRecurring("Say Hello", (ct, l) => { 
                    l.LogInformation("I'm saying hello every 5 seconds! Press Ctrl-C to start a graceful shutdown"); 
                }, 5000);
            });

        }
    }
}
