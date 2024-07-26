using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.AI;
using Perigee.Scheduler;

namespace Samples.CodeBlocks;

/*
    ---- Sync Agent Coordination ----
    - https://docs.perigee.software/core-modules/event-sources/scheduled-logic/sync-agent
    
    Sync Agent Coordination demonstrates the use of synchronization agents for unparalleled task coordination within your application.

    --== Learning Objective #1: Understanding Sync Agents ==--
    * Learn the purpose and benefits of using sync agents for task coordination.
    * Understand how sync agents can ensure tasks run only when certain conditions are met.
    * Observe the initialization of a memory-based sync agent source.

    --== Learning Objective #2: Setting Up Sync Agents ==--
    * Learn how to set up sync agents with specific run conditions and schedules.
    * Understand the importance of setting synchronization limits and intervals.
    * Observe the configuration of two sync agents: PullData and LoadExcel.

    --== Learning Objective #3: Coordinating Dependent Tasks ==--
    * Learn how to configure tasks to run only after dependent tasks have produced valid data.
    * Understand the use of late binding behavior trees to check for task completion.
    * Observe the setup of an ExecuteRefresh agent that runs after PullData and LoadExcel have completed.

    --== Learning Objective #4: Implementing Task Execution Logic ==--
    * Learn how to implement the execution logic for sync agents.
    * Understand how to handle task execution and completion
    * Observe the detailed task execution and completion process in the logs.

    --== Learning Objective #5: Error Handling and Re-scheduling ==--
    * Understand the importance of error handling and re-scheduling for failed tasks.
    * Learn how to set up re-scheduling intervals for tasks that fail to complete.
    * Observe the behavior of the sync agents when handling errors and re-scheduling.

    --== Learning Objective #6: Practical Application of Sync Agents ==--
    * Learn why using sync agents can enhance task coordination and reliability in real-world applications.
    * Understand the practical benefits of using sync agents for managing complex task dependencies.
    * Observe how sync agents ensure data validity and task synchronization.

    --> To run this sample:
    E2_SyncAgent.run();
*/

public static class E2_SyncAgent
{
    public static void run()
    {
        PerigeeApplication.ApplicationNoInit("Unparalleled Task Coordination", (c) =>
        {
            //Clear on start, for demo purposes only
            if (File.Exists("MemAgent.json")) File.Delete("MemAgent.json");

            //Source
            var AgentSource = new SyncAgentSourceMemory("MemAgent.json", c.GetCancellationToken());

            /* PullData agent */
            c.AddAgent("PullData", "PullData", "Main", AgentSource, AgentRunCondition.RunIfPastDue,
                (configAgent) => configAgent.SetSyncLimitPerDay(1).SetSync(TimeSpan.FromSeconds(5)),
                (ct, l, exec) =>
                {
                    l.LogInformation("Pulling and loading data from remote source...");
                    Task.Delay(2000).Wait();
                    l.LogInformation("Done! Data is valid until {date}", DateTimeOffset.Now.AddDays(1));
                    return exec.Complete(DateTimeOffset.Now.AddDays(1));
                },
                (ct, l, tree) => { });

            /* LoadExcel agent */
            c.AddAgent("LoadExcel", "LoadExcel", "Main", AgentSource, AgentRunCondition.RunIfPastDue,
                (configAgent) => configAgent.SetSyncLimitPerDay(1).SetSync(null, "0 */1 * * * *"),
                (ct, l, exec) =>
                {
                    l.LogInformation("Loading data from Excel...");
                    Task.Delay(2000).Wait();
                    l.LogInformation("Done! Data is valid until {date}", DateTimeOffset.Now.AddDays(1));
                    return exec.Complete(DateTimeOffset.Now.AddDays(1));
                },
                (ct, l, tree) => { });


            /* Add an agent "ExecuteRefresh" that ONLY runs after the first two have produced valid data */
            c.AddAgent("ExecuteRefresh", "ExecuteRefresh", "Main", AgentSource, AgentRunCondition.RunIfPastDue,
                (configAgent) =>
                    configAgent.SetSyncLimitPerDay(1).SetSync(TimeSpan.FromSeconds(5))
                        .SetLateBindingBehaviorTrees(true, false),
                (ct, l, exec) =>
                {
                    l.LogInformation("Starting refresh of data now that all my sources have non expired data");
                    Task.Delay(3000).Wait();
                    l.LogInformation("Done! Data is valid until {date}", DateTimeOffset.Now.AddDays(1));

                    return exec.Complete(DateTimeOffset.Now.AddDays(1));
                },
                (ct, l, tree) =>
                {
                    //Late binding tree update checker
                    if (tree.TreeType == AgentTreeType.SyncTree)
                    {
                        var BT = new BehaviorTree("Check previous level completion").AddSequence("Check expiration",

                            //Returns success if the data is not expired, allowing the sequence check to proceed
                            LeafNodes.AgentDataExpired("PullData", tree.AgentData, l),
                            LeafNodes.AgentDataExpired("LoadExcel", tree.AgentData, l));

                        //Set tree for late binding execution
                        tree.UpdateTree(BT);
                    }
                }, failedTreeReshcedule: TimeSpan.FromSeconds(15));
        });
    }
}