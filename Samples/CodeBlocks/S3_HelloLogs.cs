using Microsoft.Extensions.Logging;
using Perigee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
    /*
    ---- Logging And Scopes ----
    - https://docs.perigee.software/getting-started/hello-logs

    This example demonstrates how to use logging and log scopes within threads to customize and manage log output.

    --== Learning Objective #1: Direct Logging ==--
    * Understand how to log directly from the thread registry method
    * Observe the log output generated every 10 seconds
    * Modify the log message to see the changes reflected in the output

    --== Learning Objective #2: Recurring Logging with Scopes ==--
    * Learn how to set up recurring logging tasks
    * Understand the concept of log scopes and how to use them to override default log properties
    * Modify the log scope dictionary to include additional properties and observe the changes in the log output (hint: this will require changing the appsettings.json. See the help doc link above if you get stuck!)

    --== Learning Objective #3: Customizing Log Output ==--
    * Experiment with different log levels (e.g., LogWarning, LogError) and observe how the output changes
    * Add additional recurring loggers with different intervals and messages to see how they coexist in the log output
    * Modify the log interval to see how it affects the frequency of log messages

    --> To run this sample:
    S3_HelloLogs.run();
    */
    public static class S3_HelloLogs
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Logging And Scopes", (c) =>
            {
                //Get a direct log from c (ThreadRegistry)
                c.AddRecurring("DirectLog", (ct, l) => {
                    
                    c.GetLogger<Program>().LogInformation("I am logging directly from the thread registry method");
                }, 10000);

                c.AddRecurring("RecurringLogger", (ct, l) => {

                    //Begin a new log scope on this logger, overriding the ThreadName
                    using var scopes = l.BeginScope(new Dictionary<string, object> { { "ThreadName", "CUSTOMIZED" } });

                    //Anything now logged from here will have it's "ThreadName" set to "CUSTOMIZED"
                    l.LogInformation("See the overriden thread name?");

                });
            });
        }
    }
}
