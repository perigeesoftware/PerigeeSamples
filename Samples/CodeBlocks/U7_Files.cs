using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.FileFormats.CSV;
using Perigee.Transform;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
    /*
        ---- File Reading and Transformation ----

        File Reading and Transformation demonstrates how to read, clean, and transform files like CSV and Excel using the provided utilities for efficient data handling.

        --== Learning Objective #1: Reading and Cleaning CSV Files ==--
        * Learn how to read and clean CSV files with inconsistent data such as extra headers and junk entries.
        * Understand how to use the `Transformer.TableFromFile` method to convert a messy CSV file into a clean DataTable.
        * Observe the logging of raw file content and the cleaned version of the CSV file.

        --== Learning Objective #2: Transforming File Data Formats ==--
        * Learn how to convert file data formats from CSV to JSON or from Excel to CSV using the `Transformer.CleanFile` method.
        * Understand how the `CleanFile` utility can transform and clean file data in one step.
        * Observe the transformation of CSV data into JSON format and Excel data into CSV format.

        --== Learning Objective #3: Working with Different File Types ==--
        * Learn how to handle different file types such as CSV and Excel files.
        * Observe how the utility handles both file types, making file reading and transformation seamless.

        --== Learning Objective #4: Practical Application of File Handling and Transformation ==--
        * Learn why using utilities like `Transformer` and `CleanFile` can enhance file handling, especially with unstructured or inconsistent data.
        * Understand the practical benefits of using these utilities to automate and simplify file reading, cleaning, and transformation.
        * Observe how the utilities ensure efficient file handling, making file transformation a breeze.

        --> To run this sample:
        U7_Files.run();
    */

    public static class U7_Files
    {
        private static string _Path_PropertiesCSV = $"Files{Path.DirectorySeparatorChar}Property.csv";
        private static string _Path_PropertiesExcel = $"Files{Path.DirectorySeparatorChar}Property.xlsx";

        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Files", (c) =>
            {
                c.Add("Read Files", (ct, l) => { 

                    //Let's read a CSV file
                    DataTable PropertiesFile = Transformer.TableFromFile(_Path_PropertiesCSV);

                    //If you look at this file, it's pretty messed up. Extra headers, junk data, many newlines...
                    l.LogInformation(File.ReadAllText(_Path_PropertiesCSV));

                    //But ours is perfectly clean!
                    l.LogInformation(new CSVWriter(PropertiesFile).Write());

                    //Let's clean and convert it to json. In one line...
                    // CleanFile takes a table, or bytes, and cleans + converts the data
                    //  (Encoding is needed to convert back into a string to log)
                    l.LogInformation(
                        Encoding.UTF8.GetString(
                            Transformer.CleanFile(File.ReadAllBytes(_Path_PropertiesCSV), TransformUtil.ToMimeType(".csv"), 
                            CleanFileFormat.Json)));


                    //It reads Excel documents like this too... Let's convert it to a CSV
                    l.LogInformation(
                        Encoding.UTF8.GetString(
                            Transformer.CleanFile(File.ReadAllBytes(_Path_PropertiesExcel), TransformUtil.ToMimeType(".xlsx"),
                            CleanFileFormat.CSV)));


                    //Easy peasy file reading is breezy!


                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });


        }
    }
}
