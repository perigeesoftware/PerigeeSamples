using Perigee;

namespace Samples.CodeBlocks;

/*
    ---- Custom Methods ----
    - https://docs.perigee.software/perigee-and-beyond/extending-threads
    
    Thread Extensions / Custom Methods demonstrates how you can extend thread functionality with custom operations in your application.

    --== Learning Objective #1: Adding Custom Thread Functions ==--
    * Understand how to extend the ThreadRegistry with custom functions.
    * Learn how to add a managed thread using a custom function.
    * Observe the custom thread running and view the output in the logs.

    --== Learning Objective #2: Long-running tasks ==--
    * Learn how to create a task that runs indefinitely until explicitly stopped.
    * Implement a loop that periodically checks for cancellation.
    * Ensure the task can handle graceful shutdown requests by monitoring the cancellation token.

    --== Learning Objective #3: Task Error Handling ==--
    * Understand how to configure tasks to handle exceptions and restart automatically.
    * Learn how to set up a restart time interval for tasks after an exception.
    * Trigger an exception and observe the behavior of the thread in the application logs when an exception occurs.

    --== Learning Objective #4: Thread Management Options ==--
    * Set up and configure additional options for managed threads.
    * Learn how to add managed threads to the ThreadRegistry.

    --> To run this sample:
    T2_CustomMethods.run();
*/
public class T2_CustomMethods
{
    public static void run()
    {
        PerigeeApplication.ApplicationNoInit("Custom Method", (c) =>
        {
            //And it's available to use!
            c.AddCustomXYZFunction();
        });
    }
}

public static class T2_Extensions
{
    public static ThreadRegistry AddCustomXYZFunction(this ThreadRegistry tr)
    {   
        //Create a regular managed thread (not an expression, CRON)
        var TM = new ManagedThread("CustomXYZFunction", (ct, l) => { 
        
            //The callback method is here...
            do
            {
                //An example of repeating something every second, and exiting the thread when the token is cancelled.


                //Doing a long process that can be stopped safely? Pay attention to the cancellation token, and kindly exit when a graceful shutdown is requested.
                if (ct.IsCancellationRequested)
                {
                    //Save or finish or persist information.
                    return; 
                }

            }
            while (PerigeeApplication.delayOrCancel(1000, ct));

        }, tr.CTS, tr.GetLogger<Program>(), started: true);

        //Set additional options
        TM.ExceptionRestartTime = TimeSpan.FromSeconds(30);

        //Add the thread to the management system
        tr.AddManagedThread(TM);
    
        return tr;
    }
}