using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.FileFormats.CSV;

namespace Samples.CodeBlocks;

/*
    ---- Directory Watcher Demo ----
    - https://docs.perigee.software/core-modules/event-sources/watchers/directory-watch

    The Directory Watch is able to monitor a folder and report back when a new file is present. 
    It has several key checks in place to make sure the file isn't actively being written too, or locked by another application.
    Directory Watch expects that the file will be removed from the directory after it has been processed. 
       For this reason there are several options on the DirectoryWatchFailurePolicy on how to handle when a file hasn't been removed. 
   
    --== Learning Objective #1: Setting Up a Directory Watcher ==--
    * Learn how to initialize a directory watcher to monitor specific file types with a path.
    * Understand the importance of specifying the correct path to watch.
    * Observe the setup of a directory watcher to monitor CSV files.

    --== Learning Objective #3: Handling File Changes ==--
    * Learn how to handle file changes detected by the directory watcher.
    * Understand how to read and process CSV files upon detection.
    * Observe the logging of CSV file details such as encoding, columns, rows, delimiter, and jagged status.

    --== Learning Objective #4: Failure Policy Handling ==--
    * Understand the importance of failure policies in directory watching.
    * Learn how to configure failure policies to handle errors during file processing.
    * Observe how files are moved to a _Failed folder upon processing failure.
    * Try adjusting the failure policy and see what happens

    --> To run this sample:
    E3_DirectoryWatcher.run();
*/

public static class E3_DirectoryWatcher
{
    public static void run()
    {
        PerigeeApplication.ApplicationNoInit("Watcher Demo", (c) =>
        {

            c.AddDirectoryWatch("CSV", "C:\\Watch", "*.csv", SearchOption.AllDirectories, (ct, l, path) => {

                //Read the CSV
                var CSVData = CSVReader.ToDataTable(path, out var rRes);

                //Reprt on it
                l.LogInformation("Read CSV {file}[{encoding}]. Columns/Rows: {col}/{row}; Delimiter: {delChar}; Jagged? {jagged}", 
                    Path.GetFileName(path), rRes.FileEncoding.EncodingName, rRes.ColumnCount, 
                    CSVData.Rows.Count, rRes.FinalDelimiter, rRes.RowShifts.Count > 0 ? "YES" : "NO");

                //You'll notice the file gets moved to the _Failed Folder (Due to DirectoryWatchFailurePolicy supplied below)
                //  Watcher expects the file to be removed after it's processed to prevent infinite loops


            }, policy: ThreadRegistry.DirectoryWatchFailurePolicy.MoveToFailedFolder);

        });

    }
}