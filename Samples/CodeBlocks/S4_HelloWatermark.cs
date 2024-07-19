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
    ---- Hello Watermark ----
    - https://docs.perigee.software/getting-started/hello-integration#watermarking

    This example demonstrates how to use watermarking within a thread to manage and track application state.

    --== Learning Objective #1: Watermark Registration ==--
    * Understand how to register a watermark before using it
    * Observe how the initial watermark is set to the current date and time
    * Modify the watermark registration logic to use a different initial value and observe the changes 
    *       (will require cleaning the project first to remove \Watermarks\IntegrationOffset.json from the disk)

    --== Learning Objective #2: Updating Watermarks ==--
    * Learn how to create a recurring method that uses and updates the watermark
    * Observe the log output to see the watermark's value being logged and updated every 15 seconds
    * Modify the update interval and log message to see the effects on the application behavior

    --== Learning Objective #3: Additional Event Handlers ==--
    * Understand how to register additional handlers to receive updates on the watermark
    * Observe the log output from the additional handler and see how it responds to watermark updates
    * Modify the handler to perform different actions based on the watermark value

    --== Learning Objective #4: Application Restart and Watermark Persistence ==--
    * Stop the application, wait for a minute, and then start it again
    * Observe how the watermark automatically picks back up from where it left off
    * Understand the importance of watermark persistence in maintaining application state

    --> To run this sample:
    S4_HelloWatermark.run();
    */
    public static class S4_HelloWatermark
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Hello Watermark", (c) =>
            {
                //Register a watermark - ALWAYS do this before requesting one, we recommend putting registers at the top of your method.
                Watermarking.Register("IntegrationOffset", () => Watermark.FromDateTimeOffset(DateTimeOffset.UtcNow), (nVal) =>
                {
                    c.GetLogger<Watermark>().LogInformation("New value for {name} ({value})", nVal.Name, nVal.GetDateTimeOffset());
                });


                //Create a recurring method to use and update watermark.
                c.AddRecurring("Watermarker", (ct, l) => {

                    //Get the value
                    var wmDate = Watermarking.GetWatermark("IntegrationOffset").GetDateTimeOffset();

                    //Log it out
                    l.LogInformation("Starting from {date}", wmDate);

                    //Push the current date onto the watermark
                    Watermarking.UpdateWatermark("IntegrationOffset", Watermark.FromDateTimeOffset(DateTimeOffset.UtcNow));

                }, (int)TimeSpan.FromSeconds(15).TotalMilliseconds);


                //Register an additional handler that is able to receive updates
                c.Add("RegisterExample", (ct, l) => {

                    //Register additional handlers anywhere else, they receive updates to that watermark
                    Watermarking.RegisterAdditionalEventHandler("IntegrationOffset", (s, nVal) =>
                    {
                        l.LogInformation("RegisterExample: New value for {name} ({value})", nVal.Name, nVal.GetDateTimeOffset());
                    });

                    //Low cpu cancellable wait
                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });

            });
        }
    }
}
