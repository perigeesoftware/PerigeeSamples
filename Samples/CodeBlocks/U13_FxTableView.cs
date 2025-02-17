using Perigee.Extensions;
using Perigee.FxExpression;
using Perigee.Serializer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{

    /*
        ---- FxTableView: Advanced Windowed Aggregates & Partitioning ----

        This advanced example demonstrates how to perform complex windowed aggregates and 
        in-memory partitioning — no database required! FxTableView allows you to group data (like SQL's GROUP BY), 
        order it via custom clauses, and apply windowed aggregates (such as MIN, SUM, etc.) over your data efficiently.
        Instead of duplicating data, FxTableView manages partitions by storing indices and partition splits, 
        ensuring high performance even with large datasets.

        --== Learning Objective #1: In-Memory Partitioning & Ordering ==--
        * Understand how to partition a DataTable by one or more columns (e.g., "Type") and order the data 
          using custom order clauses (e.g., ordering by "Org").
        * Learn how FxTableView uses an efficient merge/quick sort hybrid to manage ordering without duplicating data.

        --== Learning Objective #2: Windowed Aggregates & Re-Indexing ==--
        * Explore how to compute aggregates (e.g., MIN) over a defined window (with preceding and following rows).
        * Understand the concept of original versus sorted indices and how re-indexing maps the two.

        --== Learning Objective #3: Practical Data Operations ==--
        * See how FxTableView integrates with DataTable operations by allowing you to reorder data and export it as CSV.
        * Learn how to inspect partition splits and keys for diagnostic purposes, gaining insight into the grouping of your data.

        --> To run this sample:
        U13_FxTableView.run();
    */
    public static class U13_FxTableView
    {
        public static void run()
        {
            //Declare and get sample data
            var fx = new Fx();
            fx.RegisterRoot<FxDefaultFunctions>();
            DataTable DT_Sample = Bit2.Deserialize<DataTable>(File.ReadAllBytes($"Files{Path.DirectorySeparatorChar}dtsample.bit"))!;


            //Run over the data and set the Calc field to a sum partition order rows clause
            //  These operations are run in order, allowing you to run multiple expressions, ordered from start to finish
            fx.CompileTable(DT_Sample, new Dictionary<string, string>() {
                        { "Calc", "SUM([Amount] PARTITION BY [Type] ORDER BY [Org] rows between unbounded preceding and 0)" }
                    });



            /* *************************** fxTableView *************************** */

            //U12_FxExpression showed this off, however, let's dig into one layer deeper into how this works and what the FxTableView allows you to do under the hood.
            //  FxTableView is actually what the FxEngine uses under the hood for any and all aggregate functions. It also provides a powerful set of tools to partition and order the underlying data.
            //  FxTableView uses an incredibly fast merge sort / quick sort hybrid that stores ONLY the partitioned splits along with the ordinals of the new columns.
            //  This means the data of a table with 100,000 records isn't being duplicated, only new indicies are being stored. 
            var fxView = FxTableView.PartitionWithOrderBy(DT_Sample, new[] { "Type" }, new[] { new FxOrderClause("Org", true) });

            //Iterate over the sorted / partitioned table in the correct order (BY Partition [Type] and ordered by Org asc)
            foreach (var row in fxView.Take(10))
            {
                Console.WriteLine($"{row[0]} / {row[1]}: {(double)row[2]:N2}");
            }

            //Let's take a super quick look under the hood and see what data we have stored...
            // Partitioned splits are simply a "group by", they provide context as to where a partition starts and ends within the data
            //  Splits[0] is always 0, but the remaining split indicies tell us where the partitioned data is.
            var Splits = fxView.splits;

            // Keys is an object array of the unique partitioned keys, in our case, it's a single column. So we'll only look at the first item in the sub array [["A"],["B"], ...]
            var Keys = fxView.keys;

            //Let's put these two together and see how we can log out the partitioned splits
            Console.WriteLine($"Key: {Keys[0][0]} starts at {Splits[0]}");
            Console.WriteLine($"Key: {Keys[1][0]} starts at {Splits[1]}");
            //  Prints: Key: A starts at 0
            //          Key: B starts at 25

            //Finally, order tells us in which order the original rows should be pulled.
            //  Remember fxView is an ordinal store, not a data duplication. This ordering is all we need to re-order the original table
            var Order = fxView.order;

            //Reindex is the inverse of order. Meaning:
            // - Order[0] is 15 (the "new" first row should be Rows[15])
            // - ReIndexed[15] is 0 (Where did row 15 from my original index go? It's now first)
            var ReIndexed = fxView.ReIndexedPosition;

            /* *************************** fxTableView *************************** */



            /* *************************** fxCalcuationResult *************************** */
            // Use the view to calculate an aggregate (min) over a window
            var fxAggResult = fxView.GetAggregate(FxAggregateType.Min, 2, new FxWindow() { Preceding = 2, Following = 2 });

            //Get results of the min aggregate by accessing the aggregate result by original index
            var MinValue_Original = fxAggResult.GetDecimalOriginalIndex(0);

            //Here is a perfect example to understand "Original Index" versus the new sorted index.
            // Let's look at where our original row[0] ended up after being sorted by examining the ReIndexedPosition of our view.
            int newOrdinalPosition = fxView.ReIndexedPosition[0];

            //This tells us our original row[0] should now be at 10, after being sorted/partitioned.
            //The MinValue_Original value is showing what the min aggregated result of the original row was, which is now at 10.
            //  Let's look at what we logged out though, the first 10 newly sorted rows. If we want to get the min value windowed over this, we'll need to get the decimal value from the newly sorted list
            decimal MinValue_NewSort = fxAggResult.GetDecimal(2);

            // MinValue_NewSort gives me 1,017.21, which according to the newly ordered rows we logged out, is right.
            //   It looked at a total of 5 values, 2 previous, 2 future, to get a minimum value:
            //   [-2] 7,457.12,
            //   [-1] 1,334.38,
            //   [0]  8,022.21,
            //   [1]  9,836.66,
            //   [2]  1,017.21. 
            /* *************************** fxCalcuationResult *************************** */


            //Finally, let's use the extension method to simply order our table and export a CSV.
            DT_Sample = DT_Sample.OrderTableBy(new FxOrderClause("Type"), new FxOrderClause("Org"));
            var CSV = DT_Sample.ToCSV();




        }
    }
}
