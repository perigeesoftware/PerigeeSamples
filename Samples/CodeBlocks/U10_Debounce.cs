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
        ---- Debounce Mechanism ----
        -- https://docs.perigee.software/core-modules/utility-classes/debounce

        The Debounce Mechanism demonstrates how to delay the execution of a function until after a certain period of time has passed
            since the last time it was called. This is useful in scenarios where you want to limit the frequency of certain actions, 
            such as handling rapid user input or preventing unnecessary repetitive function calls.

        --== Learning Objective #1: Understanding Debounce ==--
        * Learn how debounce delays the execution of an action until a specified time after the last invocation.
        * Understand how debounce helps limit the frequency of certain operations, especially when dealing with fast or repeated events.

        --== Learning Objective #2: Using Debounce with No Arguments ==--
        * Learn how to create a debounce function without arguments.
        * Understand how to trigger the debounce multiple times, but have the action execute only after the last bounce.
        * Observe how calling `Bounce()` repeatedly delays the final execution until the bouncing stops.

        --== Learning Objective #3: Using Debounce with Arguments ==--
        * Learn how to create a debounce function that accepts arguments.
        * Understand how the last argument provided in a series of `Bounce()` calls is passed to the action.
        * Observe how calling `Bounce(i)` with different values results in the final execution receiving the last provided argument.

        --== Learning Objective #4: Practical Application of Debounce ==--
        * Learn why debounce is useful for tasks such as preventing excessive API calls, improving performance on rapid user input, or managing repetitive tasks.
        * Understand how debounce can optimize your application by reducing the frequency of expensive operations.
        * Observe how debounce ensures efficient handling of events by triggering only after a period of inactivity.

        --> To run this sample:
        U10_Debounce.run();
    */

    public static class U10_Debounce
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Debounce Example", (c) =>
            {
                c.Add("Debounce Demo", (ct, l) =>
                {
                    // Example 1: Debounce without arguments
                    var debouncer = new Debounce(() =>
                    {
                        l.LogInformation("Debounce action executed with no arguments after delay.");
                    });

                    // Trigger the bounce multiple times; the action will only fire once after the last bounce
                    debouncer.Bounce();
                    debouncer.Bounce();

                    // Example 2: Debounce with arguments
                    var debouncer_int = new Debounce<int>((i) =>
                    {
                        l.LogInformation($"Debounce action executed with argument: {i}");
                    });

                    // Trigger the bounce multiple times with arguments; the last bounce argument (3) will be used
                    debouncer_int.Bounce(1);
                    debouncer_int.Bounce(2);
                    debouncer_int.Bounce(3);

                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });
        }
    }

}
