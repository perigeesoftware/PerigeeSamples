using Perigee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perigee.Extensions;
using Microsoft.Extensions.Logging;
using Perigee.Helpers;

namespace Samples.CodeBlocks
{

    /*
        ---- Rate Limiter ----
        -- https://docs.perigee.software/core-modules/integration-utilities/limiter

        Rate Limiter demonstrates how to set up and manage a rate limiting mechanism within your application to control the frequency of actions.

        --== Learning Objective #1: Initial Setup of Rate Limiter ==--
        * Learn how to initialize a rate limiter to control API call frequency.
        * Understand the configuration parameters such as the rate limit file, limit count, and time span.
        * Observe the setup of a rate limiter for managing API call rates.

        --== Learning Objective #2: Thread-safe Access to Limiter ==--
        * Learn how to ensure thread-safe and locked access to the rate limiter file.
        * Understand the importance of thread safety in applications with concurrent access to shared resources.
        * Observe the use of a rate limiter within a recurring task.

        --== Learning Objective #3: Implementing Rate Limiting Logic ==--
        * Learn how to test the rate limit and increment the limit count upon approval.
        * Understand how to perform actions conditionally based on the rate limit status.
        * Observe the logging of rate limit status and actions performed within the limit.

        --== Learning Objective #4: Handling Rate Limit Exceedance ==--
        * Understand how to handle scenarios where the rate limit has been exceeded.
        * Learn how to log messages when the rate limit is exhausted.
        * Observe the behavior of the application when the rate limit is reached.

        --== Learning Objective #5: Practical Application of Rate Limiting ==--
        * Learn why using a rate limiter can enhance the reliability and stability of API interactions.
        * Understand the practical benefits of using a rate limiter to prevent excessive API calls.
        * Observe how the rate limiter ensures controlled and efficient API usage.

        --> To run this sample:
        U1_Limiter.run();
    */

    public static class U1_Limiter
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Limiter", (c) =>
            {

                c.AddRecurring("Limiter", (ct, l) => {
                    
                    //Thread safe and locked access to a limiter file. Can be used or declared anywhere in the application scope
                    using var APILimit = new RateLimiter("RateLimiter.json", 2, TimeSpan.FromMinutes(1), ct);

                    //Test limit, if approved, the limit will be incremented
                    if (APILimit.CanExecute())
                    {
                        //Perform action
                        l.LogInformation("Calling API {n}/{x} executions", APILimit.GetCount(), APILimit.GetLimit());
                    }
                    else
                    {
                        //Out of executions
                        l.LogInformation("Out of executions...");
                    }

                });
            });

        }
    }
}
