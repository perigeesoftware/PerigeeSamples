using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.FxExpression;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
    /*
        ---- F(x) Expressions ----
        -- https://docs.perigee.software/core-modules/utility-classes/f-x-expressions

        F(x) Expressions demonstrate how to leverage Perigee’s powerful expression evaluator, 
        enabling users to input and execute complex formulas—much like Excel’s formula bar. 
        With F(x), you can incorporate mathematical operations, functions, column references, 
        and even custom logic to dynamically compute values.

        --== Learning Objective #1: Evaluating Expressions ==--
        * Understand how to compile and execute expressions dynamically using F(x).
        * Learn how to pass variables (e.g., column values) into expressions via an identifier callback.

        --== Learning Objective #2: Custom Functions & Performance ==--
        * See how to register custom or default function classes to extend F(x) capabilities.
        * Observe how expressions are compiled into IL for rapid execution, optimizing performance for large datasets.

        --== Learning Objective #3: DataTable Integration ==--
        * Learn how F(x) can be applied to DataTable columns to compute calculated fields.
        * Understand how to perform operations like partitioned summing and column concatenation using F(x).

        --> To run this sample:
        U12_FxExpression.run();
    */

    public static class U12_FxExpression
    {
        
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Fx Expressions", (c) =>
            {
                c.Add("Demo", (ct, l) =>
                {
                    /*
                     F(x) is a powerful expression evaluator integrated within Perigee, designed to enhance data manipulation and computation capabilities. 
                       Much like the "Formula" bar in Excel, F(x) allows users to input complex formulas incorporating functions, mathematical operations, column references, and parentheses. 
                       However, F(x) goes beyond basic evaluation by offering advanced features:

                      - Custom Functionality: Users can add custom classes with functions, expanding the evaluator's capabilities to suit specific needs.
                      - Performance Optimization: Instead of repeatedly tokenizing, parsing, and evaluating expressions, F(x) compiles expressions into Intermediate Language (IL) code. 
                         This compilation results in a direct expression tree that can be executed swiftly, making it ideal for processing large datasets efficiently.


                     F(x) can be used anywhere you want to enable a user to write their own expressions, be it a report builder, an input box that accepts math, modifying data sources in bulk, etc.
                     */


                    //Example 1) Declare a new Fx class, and register the default functions shipped alongside it.
                    Fx fx = new Fx();
                    fx.RegisterRoot<FxDefaultFunctions>();

                    //Compile a new expression
                    var fnMethod = fx.Compile("abs([a]) * 2.5");

                    //Get the results
                    FxValue fnResults = fnMethod(
                        //the idCallback will be called when an identifier [value] is found and needs to be referenced. 
                        (idCallback) =>
                        {

                            if (idCallback.Identifier == "a")
                                return FxValue.From(10m);
                            else
                                return FxValue.From(1m);
                        });

                    //prints 25.0
                    l.LogInformation("Result: {v}", fnResults);


                    //Example 2) Register our custom function and use it
                    fx.RegisterRoot<CustomFunctions>();

                    //Compile a new expression, with a method override
                    var fnMethodCustom = fx.Compile("root(6)");

                    //Get the results
                    FxValue fnResultsCustom = fnMethodCustom(
                        (idCallback) => FxValue.From(0m));

                    //prints  2.4494897
                    c.GetLogger<Program>().LogInformation("Result: {v}", fnResultsCustom);



                    //Example 3) Let's see how Fx can be used on a data table:
                    DataTable DT_Sample = new DataTable();
                    DT_Sample.Columns.Add("Org", typeof(int));
                    DT_Sample.Columns.Add("Type", typeof(string));
                    DT_Sample.Columns.Add("Amount", typeof(decimal));
                    DT_Sample.Columns.Add("Calc", typeof(decimal));
                    FillTableRows(DT_Sample, 1000);

                    //Run over the data and set the Calc field to a sum partition, and then reassign Type to a concatenation
                    //  These operations are run in order, allowing you to run a calculation first, then re-assign the Type column
                    fx.CompileTable(DT_Sample, new Dictionary<string, string>() {
                        { "Calc", "SUM([Amount] PARTITION BY [Type])" },
                        { "Type", "[Org] & '-' & [Type]"}
                    });

                    c.GetLogger<Program>().LogInformation("Result 0: {@v}", DT_Sample.Rows[0].ItemArray.ToList());


                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });
        }

        public class CustomFunctions
        {
            public static FxValue root(FxValue dbl) => FxValue.From(Math.Sqrt(dbl.AsDouble()));
        }

        public static void FillTableRows(DataTable table, int count)
        {
            string[] types = { "A", "B", "C", "D", "E" };

            Random rand = new Random();

            table.BeginLoadData();

            try
            {
                for (int i = 0; i < count; i++)
                {
                    int org = rand.Next(1, 1001); // Random integer between 1 and 1000
                    string type = types[rand.Next(types.Length)]; // Random type from the array
                    decimal amount = Math.Round((decimal)(rand.NextDouble() * 10_000), 2); // Random decimal between 0.00 and 10,000.00

                    table.Rows.Add(org, type, amount, 0.0m);
                }
            }
            finally
            {
                table.EndLoadData();
            }
        }
    }
}
