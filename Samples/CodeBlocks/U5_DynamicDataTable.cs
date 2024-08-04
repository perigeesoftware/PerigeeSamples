using System.Data;
using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Data;

namespace Samples.CodeBlocks
{

    /*
        ---- Dynamic Data Table ----
        -- https://docs.perigee.software/core-modules/utility-classes/dynamic-data-table

        Dynamic Data Table demonstrates how to dynamically load and manage data tables without worrying about data types, while providing powerful features for data manipulation and analysis.

        --== Learning Objective #1: Initial Setup of Dynamic Data Table ==--
        * Learn how to initialize and dynamically load a data table.
        * Understand how DynamicDataTable detects header information and solves data types after data is added.
        * Observe the creation of a dynamic data table and the automatic detection of column types.

        --== Learning Objective #2: Adding and Managing Data Dynamically ==--
        * Learn how to add rows to the DynamicDataTable dynamically, including header and data rows.
        * Understand the ability to load data out of order and append columns as needed.
        * Observe the process of adding data dynamically and converting it to a DataTable.

        --== Learning Objective #3: Handling and Converting Data Types ==--
        * Learn how to handle different data types within the DynamicDataTable.
        * Understand how to modify data types for specific columns and rows.
        * Observe the change in data type for a column when different types of data are added.

        --== Learning Objective #4: Logging and Analyzing Data ==--
        * Learn how to log and analyze data from the DynamicDataTable.
        * Understand how to extract and log specific column types and header information.
        * Observe the logging of data table content, including conversion to CSV format.

        --== Learning Objective #5: Calculating Statistics and Metrics ==--
        * Learn how to calculate statistics and metrics over the dataset using DynamicDataTable.
        * Understand how to retrieve and log fill rates and other metrics for the data columns.
        * Observe the calculation and logging of fill rates and column statistics.

        --== Learning Objective #6: Practical Application of Dynamic Data Tables ==--
        * Learn why using a Dynamic Data Table can enhance data handling and manipulation in real-world applications.
        * Understand the practical benefits of using a Dynamic Data Table for dynamic data loading and analysis.
        * Observe how the Dynamic Data Table ensures efficient and flexible data management.

        --> To run this sample:
        U5_DynamicDataTable.run();
    */
    public static class U5_DynamicDataTable
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Dyanmic Tables", (c) =>
            {
                c.Add("Tables Example", (ct, l) =>
                {
                    
                    //DyanmicDataTable allows you to dynamically load a table and not worry about typing.
                    // It can solve the header row by detecting the dataset below header information
                    // It calculates and stores metrics
                    // You can load data out of order, or append columns
                    // When finished, end the data load and convert to a table
                    
                    
                    //Dynamically create a data table and solve the data types after data has been added
                    DataTable Data = new DynamicDataTable()
                        .AddRowValues(0u, "ID", "Name", "Age")
                        .AddRowValues(1u, 1, "John", 25)
                        .AddRowValues(2u, 2, "Smith", 30)
                        .AddRowValues(3u, 3, "Jane", 29)
                        .FinishDataLoad().ToDataTable();
                    
                    //Show the int type for the age column
                    l.LogInformation("Column Age Type: {ageType}", Data.Columns[2].DataType.Name);
                    
                    //Now let's change the age type for one to a string, and let's also add some header information
                    var dynamicTable = new DynamicDataTable()
                        .AddRowValues(0u, "My awesome file")
                        .AddRowValues(1u, "Data as of:", "6/5/1920")
                        .AddRowValues(2u, "ID", "Name", "Age")
                        .AddRowValues(3u, 1, "John", 25)
                        .AddRowValues(4u, 2, "Smith", 30)
                        .AddRowValues(5u, 3, "Jane", "29")
                        .AddRowValues(6u, 4, "", 52).FinishDataLoad();

                    Data = dynamicTable.ToDataTable();
                    
                    //Now it's been solved to a string
                    l.LogInformation("Column Age Type: {ageType}", Data.Columns[2].DataType.Name);
                    
                    //Also, we can log out the solved header index (2) and the first row values.
                    // This successfully bypassed the header information present in the file
                    l.LogInformation("Header Index? {row} Values: {@values}", dynamicTable.HeaderRow, Data.Rows[0].ItemArray.ToList());

                    //Let's view the table as a CSV:
                    l.LogInformation("{csv}", dynamicTable.ToCSV());
                    
                    //You can easily calculate statistics over the dataset as well. Let's look at fill rates
                    var statistics = dynamicTable.GetStatistics();
                    foreach (var kvp in statistics.ColumnNames) 
                        l.LogInformation("Fill: {rate:N2}%", statistics.FillRate[kvp.Key] * 100.0m);
                    
                    
                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });
        }
    }
}