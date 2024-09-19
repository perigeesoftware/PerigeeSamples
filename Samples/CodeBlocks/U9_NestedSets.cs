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
        ---- Nested Sets ----
        -- https://docs.perigee.software/core-modules/utility-classes/nested-sets

        Nested Sets demonstrate how to efficiently represent and query hierarchical data, providing faster read operations compared to traditional adjacency lists.

        --== Learning Objective #1: Understanding Nested Sets ==--
        * Learn how nested sets can be used to represent hierarchies such as organizational structures, document trees, and file systems.
        * Understand the performance advantages of nested sets over adjacency lists for querying hierarchical data.
        * Observe how nested sets encode tree structure within the records themselves, allowing for more efficient traversal of hierarchies.

        --== Learning Objective #2: Setting Up Nested Sets ==--
        * Learn how to initialize a nested set with hierarchical data.
        * Understand how to define the root node and assign key and parent fields to create the hierarchical structure.
        * Observe the initialization of a nested set for an employee hierarchy.

        --== Learning Objective #3: Querying Uplines, Downlines, and Siblings ==--
        * Learn how to query hierarchical data to retrieve uplines (ancestors), downlines (descendants), and siblings.
        * Understand how the `Upline`, `Downline`, and `Siblings` methods allow for efficient traversal of the hierarchical data.
        * Observe how the employee hierarchy is queried to display the upline for a specific employee, the downline for a manager, and siblings of a node.

        --== Learning Objective #4: Practical Application of Nested Sets ==--
        * Learn why using nested sets can significantly enhance the performance and efficiency of hierarchical data retrieval.
        * Understand the practical benefits of using nested sets for various hierarchical structures in real-world applications.
        * Observe how nested sets ensure faster queries and simpler management of complex hierarchies.

        --> To run this sample:
        U9_NestedSets.run();
    */

    public static class U9_NestedSets
    {
        public static void run()
        {

            PerigeeApplication.ApplicationNoInit("Nested Sets", (c) =>
            {
                /*
                    Nested sets can be used anywhere a hierarchy is defined. This can be document structures, 
                        the structure of a legal document, where to stock items in a grocery store, file systems,
                        an account tree, team sales, etc.

                    Adjacency lists can be slow when iterating many items as each lookup requires additional queries 
                        or recursive operations to traverse the hierarchy, leading to increased latency and reduced 
                        performance. In contrast, nested sets allow for more efficient querying of hierarchical data 
                        by encoding the tree structure within the records themselves, enabling faster read operations and 
                        easier retrieval of entire subtrees.
                 
                 */

                c.Add("Employees", (ct, l) =>
                {

                    List<Employee> employeeList = new List<Employee>
                    {
                        // Root
                        new Employee() { id = 0, name = "CEO" },

                        // L1 nodes
                        new Employee() { id = 1, parent = 0, name = "CTO" },
                        new Employee() { id = 2, parent = 0, name = "CFO" },
                        new Employee() { id = 3, parent = 0, name = "COO" },

                        // L2 nodes
                        new Employee() { id = 4, parent = 1, name = "Engineering Manager" },
                        new Employee() { id = 5, parent = 1, name = "QA Manager" },
                        new Employee() { id = 6, parent = 2, name = "Finance Manager" },
                        new Employee() { id = 7, parent = 3, name = "Operations Manager" },

                        // L3 nodes
                        new Employee() { id = 8, parent = 4, name = "Senior Developer" },
                        new Employee() { id = 9, parent = 4, name = "Junior Developer" },
                        new Employee() { id = 10, parent = 5, name = "QA Tester" },
                        new Employee() { id = 11, parent = 6, name = "Accountant" },
                        new Employee() { id = 12, parent = 7, name = "Logistics Coordinator" },
                    };


                    // Initialize the NestedSet. This simply requires a list, the root, and the keys
                    var nsEmployee = new NestedSet<Employee>(
                        employeeList,
                        
                        // Select the first item that has an ID of 0. This is our root.
                        (l) => l.First(f => f.id == 0),
                        
                        // Select the ID(key) field, and the Parent(fk) field 
                        (f) => f.id, (f) => f.parent);



                    // Retrieve and display the upline for employee with id 9 (Junior Developer)
                    l.LogInformation("Upline for 'Junior Developer':");
                    foreach (var item in nsEmployee.Upline(9, true))
                        l.LogInformation($"[{item.HLevel}]({item.ID}) {item.Node.name}");

                    // Retrieve and display the downline for employee with id 1 (CTO)
                    l.LogInformation("\nDownline for 'CTO':");
                    foreach (var item in nsEmployee.Downline(1, true))
                        l.LogInformation($"[{item.HLevel}]({item.ID}) {item.Node.name}");

                    // Retrieve and display the siblings for employee with id 8 (Senior Developer)
                    l.LogInformation("\nSiblings for 'Senior Developer':");
                    foreach (var item in nsEmployee.Siblings(8, true))
                        l.LogInformation($"[{item.HLevel}]({item.ID}) {item.Node.name}");




                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });

            });

        }

        public class Employee
        {
            public int id { get; set; }
            public int parent { get; set; }
            public string name { get; set; }
        }
    }
}
