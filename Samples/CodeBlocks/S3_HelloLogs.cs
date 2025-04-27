using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Perigee;
using Perigee.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    --== Learning Objective #4: Metrics ==--
    * Let's understand how the metrics system works, and play with logging out the results
    * Try changing the average time to say, Minimum, Maximum, or even Nth Percentile
    * Learn more about the Metrics system at: https://docs.perigee.software/core-modules/utility-classes/metrics
    

    --> To run this sample:
    S3_HelloLogs.run();
    */
    public static class S3_HelloLogs
    {
        public static void run()
        {
            PerigeeApplication.App("Logging And Scopes", (c) =>
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

                c.AddRecurring("Metrics", (ct, l) => {

                    //Let's look at metrics and log them out.
                    //  First, let's get this running thread and assign a new metric to it
                    var thread = c.GetThread("Metrics");

                    //We can easily keep track of the current run count of this thread by calling increment and giving it a name
                    thread.IncrementMetric("RunCount");

                    //Let's log out the current run count. Notice this uses Int type? Increment metrics create integer types
                    l.LogInformation("{name} has run {n} times", thread.Name, thread.TryGetMetricInt("RunCount")?.Sum() ?? 0);

                    //Let's look at adding a decimal type. Let's simulate a random time to use for the process time.
                    //  We name it "ProcessTime"
                    //  Assign a random number (simulating a process time)
                    //  Then supply "1" as the rounding precision.
                    thread.AddMetricDecimal("ProcessTime", (decimal)Random.Shared.NextDouble() * 3, 1);

                    //Let's log out the average:
                    l.LogInformation("{name} average run time: {n:n2}", thread.Name, thread.TryGetMetricDecimal("ProcessTime")?.Average() ?? 0);

                    //Get the full json of the Metrics thread, all metrics from all threads, and then a jObject we can format indented. 
                    // Plenty of options to retrieve metrics!
                    // These are useful for logging, dashboards, shipping to an API, etc.
                    var ProcessTimeJson = thread.TryGetMetricDecimal("ProcessTime").AsJson(false);
                    var AllMetrics = c.GetMetricsJson(false);
                    var ThreadMetrics = thread.GetMetricsAsJObject(false).ToString(Formatting.Indented);



                }, TimeSpan.FromSeconds(2));


            });
        }
    }
}
