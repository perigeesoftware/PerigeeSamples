using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Scheduler;

namespace Samples.CodeBlocks;

/*
    ---- Scheduler Demo ----
    - https://docs.perigee.software/core-modules/event-sources/scheduled-logic/scheduler

    Scheduler Demo illustrates how to set up and manage scheduled tasks within your application.

    --== Learning Objective #1: Setting Up a Memory Scheduled Source ==--
    * Understand how to declare and initialize a memory-based scheduled source.
    * Learn the importance of using a single instance of memory/file-based sources to avoid locking issues.
    * Observe the creation of a memory source for scheduling tasks.

    --== Learning Objective #2: Adding Scheduled Items ==--
    * Learn how to add scheduled items to the memory source.
    * Understand the scheduling syntax and parameters used for scheduling tasks at specific intervals.
    * Observe the addition of two scheduled items with different intervals (15 seconds and 45 seconds).

    --== Learning Objective #3: Handling Scheduled Tasks ==--
    * Learn how to differentiate and handle tasks based on their types.
    * Understand how to log information for different task types using the provided logger.
    * Observe the behavior of the scheduler as it logs task execution information.
 
    --== Learning Objective #4: Replace with MSSQL source ==--
    * Look at the documentation and copy the configuration for the MSSQL source
    * - https://docs.perigee.software/core-modules/event-sources/scheduled-logic/scheduler#mssql-source 
    * Start the application, and add records to the table in MSSQL
    * Watch as the scheduler manages the scheduling live 

    --> To run this sample:
    E1_Schedule.run();
*/
public static class E1_Schedule
{
    public static void run()
    {
        PerigeeApplication.ApplicationNoInit("Event Scheduler", (c) =>
        {
   
            //Declare a new memory source, remember to use a single instance of memory/file based sources or locking can occur
            using var MemSource = new MemoryScheduledSource("memSource.json", c.CTS.Token);

            //Add scheduled items.
            //If this was something like a DatabaseScheduledSource, we obviously would control these records from the database, not here.
            MemSource.AddIfNotExists(GenericScheduledItem<ushort>.MemoryItem(0, "A scheduler, 15sec", "A", "a;b;c", "*/15 * * * * *", TimeZoneInfo.Local));
            MemSource.AddIfNotExists(GenericScheduledItem<ushort>.MemoryItem(1, "B scheduler, 45sec", "B", "b;c;d", "45 * * * * *", TimeZoneInfo.Local));

            //Add a scheduler with the MemorySource, a single callback is given for anything required to run (multi-threaded)
            c.AddScheduler("Main", MemSource, (ct, l, item) => { 
                if (item.GetRunType() == "A")
                {
                    l.LogInformation("Running A with {args}", item.GetRunArgs());
                }
                else if (item.GetRunType() == "B")
                {
                    l.LogInformation("Running B with {args}", item.GetRunArgs());
                }

            });

        });

    }
}