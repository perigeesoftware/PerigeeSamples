using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Perigee;
using Perigee.Serializer;
using Perigee.Transform;
using Samples.Utils;
using System.Data;

namespace Samples.CodeBlocks
{

    /*
        ---- Bit Serialization ----
        -- https://docs.perigee.software/core-modules/bit-serializer

        Bit Serialization demonstrates the powerful capabilities of the `Bit` library for efficient, flexible serialization 
          and deserialization of data, which is essential for saving disk space, reducing memory usage, and optimizing network traffic.

        --== Learning Objective #1: Introduction to Bit Serialization ==--
        * Learn how `Bit` serialization enables efficient storage and transmission of data by reducing its size without sacrificing data integrity.
        * Understand the performance benefits of using `Bit` over traditional formats like JSON, Parquet, or Avro, especially for compressing data in memory or on disk.

        --== Learning Objective #2: Serializing and Deserializing Data Tables ==--
        * Learn how to serialize complex structures like DataTables using `Bit`, reducing their size significantly in memory and on disk.
        * Understand the ease of deserializing compressed data back into usable formats, like DataTables.

        --== Learning Objective #3: Serializing and Deserializing Custom Classes ==--
        * Learn how to serialize and deserialize custom classes, such as `BitPerson`, using the `Bit` library.
        * Understand how to save memory and disk space, with `Bit` achieving compression rates of up to 90% compared to raw JSON.

        --== Learning Objective #4: Deep Cloning and Partial Deserialization ==--
        * Learn how `Bit` can be used for deep cloning objects.
        * Understand how to remap and deserialize only the fields needed for a partial class from a previously serialized object using maps.
        * Observe how maps help guarantee data consistency when class structures evolve over time.

        --== Learning Objective #5: Handling Versioning and Data Format Evolution ==--
        * Learn how `Bit` handles versioning, allowing old data formats to be re-serialized into new formats using maps.
        * Understand how the built-in header of `Bit` provides compression and versioning information, making it easy to manage evolving data structures.

        --== Learning Objective #6: Using Custom Serialization Maps and Caching ==--
        * Learn how to implement custom maps for handling versioned data serialization.
        * Understand the importance of caching for improving compression and deserialization performance, up to 250x faster than without caching.

        --> To run this sample:
        U11_Bit.run();
    */
    public static class U11_Bit
    {
        internal static byte[] BitBandit_R0_Bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x2A, 0x2B, 0x06, 0x42, 0x61, 0x6E, 0x64, 0x69, 0x74, 0x01, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x18, 0x05, 0x42, 0x69, 0x6E, 0x67, 0x6F, 0x01, 0x00, 0x00, 0x00 };

        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Bit Serialization Example", (c) =>
            {
                c.Add("Serialization Demo", (ct, l) =>
                {
                    // Example 1: Serializing and Deserializing a DataTable
                    DataTable PropertiesFile = Transformer.TableFromFile($"Files{Path.DirectorySeparatorChar}Property.csv");

                    // Sizing information
                    var diskSize = new FileInfo($"Files{Path.DirectorySeparatorChar}Property.csv").Length;
                    var dtBytes = Bit2.Serialize(PropertiesFile);
                    l.LogInformation("Original Disk Size: {size} bytes, Bit Serialized Size: {bitSize} bytes", diskSize, dtBytes.Length);

                    // Deserializing the DataTable
                    var DTProperties = Bit2.Deserialize<DataTable>(dtBytes);
                    l.LogInformation("Deserialized DataTable: {rowCount} rows", DTProperties.Rows.Count);

                    // Example 2: Serializing and Deserializing a Custom Class (BitPerson)
                    var bitBandit = new BitPerson()
                    {
                        Name = "Bandit",
                        Age = 43,
                        PersonType = BitPersonType.adult,
                        RelatedPeople = new List<BitPerson> { new BitPerson() { Name = "Bingo", PersonType = BitPersonType.child } }
                    };

                    var bbBytes = Bit2.Serialize(bitBandit);
                    bitBandit = Bit2.Deserialize<BitPerson>(bbBytes);
                    l.LogInformation("Deserialized BitPerson: {name}, Age: {age}", bitBandit.Name, bitBandit.Age);

                    // Comparison with JSON serialization
                    decimal compSize = Bit2.SerializeHeadless(bitBandit).Length / (decimal)JsonConvert.SerializeObject(bitBandit).Length;
                    l.LogInformation("Bit is about {percent:N2}% smaller than JSON", (1 - compSize) * 100);

                    // Example 3: Deep Cloning with Bit
                    var bitBandit_Cloned = Bit2.DeepClone(bitBandit);
                    l.LogInformation("Cloned BitPerson: {name}, Age: {age}", bitBandit_Cloned.Name, bitBandit_Cloned.Age);

                    // Example 4: Versioning and Partial Deserialization
                    var ReMappedMap = Bit2.GetMap(SerializationInfo.Instance.RevisionMaps(0))
                        .Remap<PartialBitPerson, BitPerson>().MapToBytes();

                    var partialBandit = Bit2.Deserialize<PartialBitPerson>(bbBytes, ReMappedMap);
                    l.LogInformation("Partially Deserialized BitPerson: {name}", partialBandit.Name);

                    // Example 5: Reserializing old data format to a new version
                    var bbytes_r1 = Bit2.Reserialize<BitPerson>(BitBandit_R0_Bytes, SerializationInfo.Instance.RevisionMaps(0), SerializationInfo.Instance.RevisionMaps(1),
                        (item) => {
                            item.NewField = $"{item.Name} New";
                            return item;
                        });

                    // Fast serialization using cached maps and versioning
                    DateTime st = DateTime.Now;
                    var FastestCompression = Bit2.Serialize(bitBandit, SerializationInfo.Instance.PersonMap, SerializationInfo.Instance.Revision, SerializationInfo.Instance.PersonCache);
                    var FastestDecompression = Bit2.Deserialize<BitPerson>(FastestCompression, SerializationInfo.Instance.PersonMap, SerializationInfo.Instance.PersonCache);
                    var ts = DateTime.Now - st;

                    l.LogInformation("Fast serialization and deserialization completed: {ms:N2}ms", ts.TotalMilliseconds);

                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
            });
        }
    }

    public class SerializationInfo
    {
        private static Lazy<SerializationInfo> lazy = new Lazy<SerializationInfo>(() => new SerializationInfo());
        public static SerializationInfo Instance { get { return lazy.Value; } }

        public SerializationInfo()
        {
            var dsc = Bit2.DeSerCacheFromObject(
                new BitPerson()
                {
                    Name = "Bandit",
                    RelatedPeople = new List<BitPerson>
                   {
                       new BitPerson { PersonType = BitPersonType.child, Name = "Bingo" }
                   }
                });
            dsc.map.version = Revision;
            this.PersonCache = dsc.cache;
            this.PersonMap = dsc.map;

        }

        public ushort Revision { get; } = 0;

        public Bit2DeSerCache PersonCache { get; set; }
        public BitMap PersonMap { get; set; }


        /// <summary>
        /// This allows us to read in old versions of maps. 
        /// It can auto import old revisions to the new models.
        /// R0 is initial release
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public byte[] RevisionMaps(ushort r)
        {
            //Revision 0 - Release map
            /*
                If you updated the structure of person (adding a property), regenerate a new revision 
                  of the map and add this to the switch below.

                var str = Bit2.MapToCSharpByteString(typeof(BitPerson), Convert.ToUInt16(SerializationInfo.Instance.Revision + 1));

                
            */

            var latest = new byte[] { 0x09, 0x00, 0x00, 0x00, 0x03, 0x1B, 0x1D, 0x01, 0x00, 0xC4, 0x87, 0x3F, 0xD7, 0xFB, 0xEB, 0xA6, 0x01, 0x0F, 0x70, 0x8D, 0x6B, 0x90, 0x2A, 0xBB, 0xD2, 0xB3, 0xBC, 0xE5, 0xDF, 0x4F, 0x08, 0x41, 0xAB, 0xA6, 0xF0, 0x09, 0xD1, 0x64, 0x13, 0xCD, 0xA6, 0x28, 0xAE, 0x09, 0x59, 0x6A, 0x6F, 0x37, 0xCB, 0xE3, 0xCA, 0x43, 0x3C, 0x94, 0x84, 0x69, 0xBF, 0xB5, 0x99, 0x80, 0x00, 0x70, 0xB0, 0x85, 0x44, 0x4C, 0xCF, 0xF7, 0x27, 0x68, 0xA7, 0x32, 0xAC, 0xAF, 0x77, 0xF3, 0x8C, 0xEF, 0x17, 0x31, 0x83, 0x07, 0xE0, 0x93, 0x0E, 0x9E, 0x4B, 0xE6, 0xF7, 0xF9, 0x5D, 0x23, 0x8A, 0xE1, 0xF0, 0x87, 0x4E, 0x76, 0x36, 0xAC, 0x9D, 0xD7, 0x29, 0x5B, 0x99, 0x66, 0x4E, 0x19, 0x21, 0x01, 0xE9, 0x42, 0xB5, 0xFE, 0xCA, 0x73, 0x7D, 0x16, 0xA3, 0x78, 0x55, 0x78, 0x0A, 0x00, 0xBB, 0x5E, 0xC2, 0xCE, 0x3D, 0x98, 0xD2, 0xFF, 0x84, 0x51, 0x4B, 0x77, 0x53, 0x7F, 0x24, 0x17, 0xD3, 0x24, 0x3F, 0x42, 0x15, 0x78, 0xAD, 0xB2, 0xAB, 0xDB, 0x86, 0x40, 0xBD, 0x80, 0xFC, 0x2A, 0xA2, 0x46, 0x1A };
            switch (r)
            {
                case 0:
                    return new byte[] { 0x09, 0x00, 0x00, 0x00, 0x02, 0x1B, 0xFF, 0x00, 0x00, 0xC4, 0xA0, 0x73, 0xF7, 0xEB, 0xA6, 0x01, 0x0F, 0x30, 0xBD, 0x41, 0xAA, 0xAC, 0x78, 0xA0, 0x38, 0xCB, 0xDB, 0xFA, 0xEF, 0x53, 0xF8, 0x18, 0xB5, 0x6A, 0xCA, 0x93, 0x10, 0x7F, 0x36, 0xCD, 0x64, 0x9B, 0xEC, 0x9A, 0x90, 0xA5, 0xC6, 0xB6, 0x49, 0xDC, 0xA0, 0x38, 0xB9, 0x08, 0xF2, 0x26, 0x6D, 0x91, 0x10, 0x00, 0xFF, 0x16, 0x12, 0x31, 0xDD, 0xEE, 0x2F, 0xD0, 0xBA, 0xD2, 0x4E, 0x17, 0xEC, 0xEC, 0xEE, 0xF7, 0x30, 0x93, 0x03, 0xE0, 0xE3, 0x0F, 0x9E, 0x4B, 0x86, 0x7B, 0x7B, 0x27, 0x8F, 0x6A, 0x38, 0xF2, 0xA0, 0x93, 0x45, 0x98, 0x75, 0xF3, 0x22, 0x25, 0xAB, 0xD0, 0xCC, 0x19, 0x23, 0x69, 0x8C, 0x55, 0xE1, 0x29, 0x00, 0xEC, 0x02, 0x25, 0x76, 0xB2, 0x6A, 0xF2, 0xF8, 0x08, 0xA3, 0x96, 0xE7, 0xE7, 0x7A, 0x49, 0x26, 0x86, 0xC9, 0x7E, 0x44, 0x26, 0xF0, 0x5A, 0xF6, 0xA3, 0xA3, 0x86, 0x40, 0xBD, 0x62, 0xC2, 0xB2, 0xA1, 0x46, 0x1A }; 
                default:
                    return latest;
                    break;
            }
        }

        public static BitPerson? Read(byte[] b)
        {
            try
            {
                Bit2.ReadHeader(b, out var v);

                return Bit2.Deserialize<BitPerson>(b,
                    SerializationInfo.Instance.RevisionMaps(v),
                    SerializationInfo.Instance.PersonCache);
            }
            catch
            {
                return null;
            }
        }
    }


}
