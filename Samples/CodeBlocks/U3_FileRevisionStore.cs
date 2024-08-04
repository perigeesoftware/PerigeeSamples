using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;
using Samples.Utils;

namespace Samples.CodeBlocks
{

    /*
        ---- File Revision Store ----
        -- https://docs.perigee.software/core-modules/file-system-storage/file-revision-store

        File Revision Store demonstrates how to set up and manage file versioning and transactional updates to ensure data integrity and prevent corruption.

        --== Learning Objective #1: Initial Setup of File Revision Store ==--
        * Learn how to initialize a File Revision Store to manage file versioning.
        * Understand the importance of using a system-level transactional replace to prevent partial file writes or data corruption.
        * Observe the setup of a File Revision Store for managing file revisions and transactional updates.

        --== Learning Objective #2: Saving and Verifying Files ==--
        * Learn how to save files using the File Revision Store with a globally unique thread lock for concurrency safety.
        * Understand the verification method used to ensure data was written properly to disk.
        * Observe the process of saving a file and verifying its integrity.

        --== Learning Objective #3: Reading and Validating Files ==--
        * Learn how to read files from the File Revision Store with validation.
        * Understand the verification process used to ensure the integrity of the read data.
        * Observe the process of reading and validating a file before using its data.

        --== Learning Objective #4: Transactional File Updates ==--
        * Learn how to perform transactional updates to files using the File Revision Store.
        * Understand the concept of a single transactional lock to read, update, and save files.
        * Observe the process of updating a file with verification checks before and after the update.

        --== Learning Objective #5: Handling Corrupt Files ==--
        * Understand the ability of the File Revision Store to rewind corrupt files back to a previous version when verification fails.
        * Learn how to handle scenarios where file validation fails during read or write operations.
        * Observe how the File Revision Store ensures data integrity by rolling back to previous versions when necessary.

        --== Learning Objective #6: Practical Application of File Revision Store ==--
        * Learn why using a File Revision Store can enhance data integrity and reliability in real-world applications.
        * Understand the practical benefits of using a File Revision Store for managing file versioning and transactional updates.
        * Observe how the File Revision Store ensures controlled and efficient file handling.

        --> To run this sample:
        U3_FileRevisionStore.run();
    */
    public static class U3_FileRevisionStore
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Revision Store", (c) =>
            {
                c.Add("Revision Store", (ct, l) =>
                {
                    //FRS is used here to save a file, it uses a globally unique thread lock and is concurrent safe.
                    // Data file versioning with a system-level transactional replace to prevent partial file writes or data corruption
                    // Ability to rewind corrupt files back to a previous version when verification fails

                    //  The BitPerson class is stored using the perigee helper JsonCompress.
                    //  The verification method verifies the data was written properly to disk.
                    bool isSaved = FileRevisionStore.SaveFile("U3Data.bin",
                        JsonCompress.Compress(new BitPerson() { Name = "Bandit", PersonType = BitPersonType.adult }),
                        (b) =>
                        {
                            var d = JsonCompress.Decompress<BitPerson>(b);
                            return true;
                        });

                    l.LogInformation("Saved file? {saved}", isSaved ? "Saved" : "Not Saved");

                    //Now let's read the bytes back in, this uses the verification method before returning
                    byte[] ReadBytes = FileRevisionStore.ReadFile("U3Data.bin", out var validationFailed,
                        (b) =>
                        {
                            var d = JsonCompress.Decompress<BitPerson>(b);
                            return true;
                        });

                    if (!validationFailed)
                    {
                        var bitPerson = JsonCompress.Decompress<BitPerson>(ReadBytes);
                        l.LogInformation("Read back in {name}", bitPerson.Name);
                    }

                    //Finally, let's demonstrate the concept of a single transactional lock to read, update and save.
                    // Verification is called twice here, once on read, once on write.
                    //  Failing read verification will not proceed to update
                    //  Failing write verification will roll back the file to the previous version (if exists)
                    FileRevisionStore.UpdateFile("U3Data.bin",
                        out bool validationfailed,
                        (byteIn) =>
                        {
                            var d = JsonCompress.Decompress<BitPerson>(byteIn);
                            d.RelatedPeople = new List<BitPerson>()
                            {
                                new BitPerson()
                                {
                                    Name = "Bingo",
                                    PersonType = BitPersonType.child
                                }
                            };
                            return JsonCompress.Compress(d);
                        },
                        (b) =>
                        {
                            var d = JsonCompress.Decompress<BitPerson>(b);
                            return true;
                        });

                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });
        }
    }
}