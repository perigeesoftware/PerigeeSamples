using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
    /*
        ---- Behavior Trees ----
        -- https://docs.perigee.software/core-modules/utility-classes/behavior-trees

        Behavior Trees demonstrate how to model decision-making processes by creating a "brain" for AI characters that can simulate real-world behaviors with states, priorities, and dynamically changing needs.

        --== Learning Objective #1: Understanding Behavior Trees ==--
        * Learn how behavior trees are used to structure decision-making logic for various use cases, from robotics to AI in video games.
        * Understand the core concepts of behavior trees, such as fallback and sequence nodes.
        * Observe how behavior trees allow complex, ordered decision-making based on the success or failure of certain conditions.

        --== Learning Objective #2: Creating and Configuring Behavior Trees ==--
        * Learn how to create a behavior tree to model the behavior of a video game character.
        * Understand how to configure tree nodes, including fallback and sequence nodes, to handle different decision-making scenarios.
        * Observe how the behavior tree checks for combat and non-combat activities based on dynamic conditions.

        --== Learning Objective #3: Implementing Node Logic ==--
        * Learn how to implement custom logic in leaf nodes to control actions based on conditions like "Is the character hungry?" or "Are there enemies nearby?"
        * Understand how sequences process actions in order and fallback nodes provide alternatives when conditions fail.
        * Observe the behavior of the tree as it executes different branches based on the current game state.

        --== Learning Objective #4: Ticking the Behavior Tree ==--
        * Learn how to periodically "tick" the behavior tree to simulate ongoing decision-making.
        * Understand how tree states change dynamically as conditions (such as enemies or hunger) are modified.
        * Observe the periodic execution of the behavior tree and the logging of its current state.

        --== Learning Objective #5: Practical Application of Behavior Trees ==--
        * Learn why behavior trees are an effective way to model decision-making in real-world applications, especially in AI systems.
        * Understand the practical benefits of behavior trees for dynamic, state-driven systems that require flexible decision-making.
        * Observe how behavior trees ensure structured, clear, and efficient decision-making processes.

        --> To run this sample:
        U8_BehaviorTrees.run();
    */
    public static class U8_BehaviorTrees
    {
        public static void run()
        {
            
            PerigeeApplication.ApplicationNoInit("Behavior Trees", (c) => {
                c.Add("BT", (ct, l) => {

                    // Behavior trees are fantastic for everything from robotics to simulating real world behaviors.
                    //  Trees allow you to code a "brain" with states, priorities, and even dynamically sort needs based on current priority.
                    // So what better way to explain behavior trees than imagine we're coding a video game character!


                    //Here's our list of boolean values, which indicate "states"
                    bool Enemies = true;
                    bool Attackable = true;

                    bool Hungry = false;
                    bool Thirsty = true;
                    int EnemiesAttacked = 0;

                    /* 
                        This behavior tree allows us to specificy ordered sequences. 

                        "Fallback" nodes process each child in order until one succeeds
                        "Sequence" nodes process each child ONLY if the previous was a success. 
                                    If any child fails, it reverts back to the beginning on next tick

                        With this knowledge, we can easily see how this tree is structured.


                        We start with a fallback, allowing for the next item to proceed on failure.
                        This allows us to check for enemies first. If there are enemies around, 
                            we aren't safe to perform non combat activities, so we tackle that first.

                        If the "Enemies check" fails, then we can check non combat activities.
                        
                        The sequences are important here, because we don't want to eat food if we aren't hungry.
                            We don't want to move in for combat if there aren't enemies around


                        Try playing with the variables and see how the trees process through.


                    */

                    var btt = new BehaviorTree("AI").AddFallback("Activities",

                        new Fallback("Check for enemies",
                            new Sequence("Any enemies around?",
                                new Leaf("Enemy Checker", (e) => { l.LogInformation("Enemy Checker"); return Enemies ? NodeStatus.SUCCESS : NodeStatus.FAILURE; }),
                                new Leaf("Enemy Attackable?", (e) => { l.LogInformation("Enemy Attackable?"); return Attackable ? NodeStatus.SUCCESS : NodeStatus.FAILURE; }),
                                new Leaf("Move in for combat", (e) =>
                                {
                                    l.LogInformation("Move in for combat");
                                    EnemiesAttacked++;
                                    if (EnemiesAttacked >= 3)
                                        Enemies = false;
                                    return NodeStatus.SUCCESS;
                                })
                            )
                        ),

                        new Fallback("Non Combat Activities",
                            new Sequence("Am I hungry?",
                                new Leaf("Hungry test", (e) => { l.LogInformation("Hungry test"); return Hungry ? NodeStatus.SUCCESS : NodeStatus.FAILURE; }),
                                new Leaf("Hungry - eat food", (e) =>
                                {
                                    l.LogInformation("Hungry - eat food");
                                    Hungry = false;
                                    return NodeStatus.SUCCESS;
                                })),
                            new Sequence("Am I Thirsty?",
                                new Leaf("Thirsty test", (e) => { l.LogInformation("Thirsty test"); return Thirsty ? NodeStatus.SUCCESS : NodeStatus.FAILURE; }),
                                new Leaf("Thirsty - drink", (e) =>
                                {
                                    l.LogInformation("Thirsty - drink");
                                    Thirsty = false;
                                    return NodeStatus.SUCCESS;
                                })),
                            new Leaf("idle", (e) => { l.LogInformation("Wander around, idle, nothing to do!"); return NodeStatus.SUCCESS; })
                        )

                    ); 

                    //Print the tree
                    l.LogInformation(btt.Print());

                    //Tick every three seconds. Feel free to pause, edit booleans, or restart
                    while (PerigeeApplication.delayOrCancel(3000, ct))
                    {
                        l.LogInformation($"Ticking tree: {Enum.GetName(BTTickEngine.RunOnce(btt))}");
                    }

                    // This is a fun example of using a tree. But you can see how powerful trees are for decision making. 
                    // Feel free to play with the tree and add more to it!

                    // - Try adding a shuffle: https://docs.perigee.software/core-modules/utility-classes/behavior-trees#shuffle
                    // - Try adding a sort: https://docs.perigee.software/core-modules/utility-classes/behavior-trees#sort

                });

            });
        }
    }
}
