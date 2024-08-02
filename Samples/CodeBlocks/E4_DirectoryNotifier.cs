using Microsoft.Extensions.Logging;
using Perigee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{

    /*
        ---- Directory Notifier ----
        -- https://docs.perigee.software/core-modules/event-sources/watchers/directory-notifier

        Directory Notifier demonstrates how to set up a notifier to monitor a directory for changes in real-time.

        --== Learning Objective #1: Initial Setup of Directory Notifier ==--
        * Learn how to initialize a directory notifier to watch for specific file types.
        * Understand the importance of the directory path and file filters used for monitoring.
        * Observe the setup of a directory notifier to watch JSON and csv files in a specified directory (using regex).

        --== Learning Objective #2: NotifyInitial Flag Usage ==--
        * Learn how the `NotifyInitial` flag affects the behavior of the directory notifier.
        * Understand that if `NotifyInitial` is set to true, the notifier will alert on items currently in the folder.
        * Observe how the notifier behaves differently when this flag is set to false, only notifying about changes to existing items or new items

        --== Learning Objective #3: Handling File Changes ==--
        * Learn how to handle notifications for file changes, additions, and deletions.
        * Understand the importance of verifying file existence before processing.
        * Observe the logging of file change events, including modifications, additions, and removals.

        --== Learning Objective #4: Practical Application of Directory Notifier ==--
        * Learn why using a directory notifier can enhance real-time file monitoring in applications.
        * Understand the practical benefits of using a directory notifier for dynamic file handling tasks.
        * Observe how directory notifiers ensure immediate response to file system changes.

        --> To run this sample:
        E4_DirectoryNotifier.run();
    */
    public static class E4_DirectoryNotifier
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Notifier", (c) => {

                c.AddDirectoryNotifier("Notify Folder", @"C:\Watch", @".*\.json$|.*\.csv$", SearchOption.TopDirectoryOnly,
                    (ct, l, path) => {

                        //Before loading or reading, verify it's existance:
                        if (File.Exists(path))
                        {
                            //Added / Modified and no longer being written to
                            l.LogInformation("{file} has been modified or added", Path.GetFileName(path));
                        }
                        else
                        {
                            //Removed
                            l.LogInformation("{file} was removed", Path.GetFileName(path));
                        }

                    },
                    true, null, null, NotifyInitial: true, started: true);

            });
        }
    }
}
