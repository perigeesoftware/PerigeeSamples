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
        ---- Retry Mechanism ----
        -- https://docs.perigee.software/core-modules/utility-classes/perigee-utility-class#retry-and-rety-async

        Retry Mechanism demonstrates how to set up and manage a retry logic within your application to handle transient errors.

        --== Learning Objective #1: Initial Setup of Retry Mechanism ==--
        * Learn how to setup a retry.
        * Understand the configuration parameters such as the number of retries and the interval between retries.
        * Observe the setup of a retry mechanism for managing transient errors.

        --== Learning Objective #2: Implementing Retry Logic ==--
        * Learn how to implement retry logic that attempts an operation multiple times.
        * Understand how to configure the retry mechanism to handle exceptions and retry the operation.
        * Observe the logging of each retry attempt and the exceptions thrown.

        --== Learning Objective #3: Handling Retry Exhaustion ==--
        * Understand how to handle scenarios where the maximum number of retries is exceeded.
        * Learn how to log messages when all retry attempts have been exhausted.
        * Observe the behavior of the application when the retry limit is reached.

        --== Learning Objective #4: Practical Application of Retry Mechanism ==--
        * Learn why using a retry mechanism can enhance the resilience and reliability of your application.
        * Understand the practical benefits of using a retry mechanism to handle transient failures.
        * Observe how the retry mechanism ensures controlled and efficient error handling.

        --> To run this sample:
        U2_Retry.run();
    */
    public static class U2_Retry
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Retry", (c) => {

                c.AddRecurring("Recurring", (ct, l) => {

                    //This will be tried 5 times, with 10 seconds between retries. 
                    Tuple<bool, Exception> Retry = PerigeeUtil.Retry(5, (i) => {

                        bool Success = false;
                        if (Success == false)
                        {
                            l.LogError("Throwing an exception on retry {i}", i);
                            throw new Exception("Do or do not, there is a retry");
                        }

                    }, 3000); //3000 is 3 seconds between

                    if (!Retry.Item1)
                    {
                        l.LogError(Retry.Item2, "Retries exceeded");
                    }

                });
            });
        }
    }
}
