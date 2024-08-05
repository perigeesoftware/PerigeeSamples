using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;

namespace Samples.CodeBlocks
{

    /*
        ---- Parallel Processing ----
        -- https://docs.perigee.software/core-modules/perigee-in-parallel/parallel-processing-reference

        Parallel Processing demonstrates how to set up and manage parallel processing of data within your application for efficient and fast operations.

        --== Learning Objective #1: Initial Setup of Parallel Processing ==--
        * Learn how to initialize parallel processing within an application.
        * Understand the configuration and usage of parallel processing for handling large datasets efficiently.
        * Observe the setup of a parallel processing example with a list of movies.

        --== Learning Objective #2: Group Processing with Parallel Execution ==--
        * Learn how to use parallel processing to group items based on a key.
        * Understand how to perform group processing using a Group Processor.
        * Observe the logging of grouped items and their titles.

        --== Learning Objective #3: Single Processing with Parallel Execution ==--
        * Learn how to use parallel processing for single item lookup.
        * Understand the difference between group processing and single processing.
        * Observe the process of looking up and retrieving items using a Single Processor.

        --== Learning Objective #4: Efficient Data Iteration and Lookup ==--
        * Learn how to iterate and process data efficiently using parallel processing.
        * Understand the benefits of using concurrent-safe objects for data handling.
        * Observe the creation and logging of a concurrent-safe bag with processed data.

        --== Learning Objective #5: Practical Application of Parallel Processing ==--
        * Learn why using parallel processing can significantly enhance the performance of data-intensive applications.
        * Understand the practical benefits of using parallel processing for fast and efficient data handling.
        * Observe how parallel processing ensures controlled and efficient data iteration and lookup.

        --> To run this sample:
        U4_Parallel.run();
    */

    public static class U4_Parallel
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("Parallel", (c) =>
            {

                c.Add("Parallel Process", (ct, l) =>
                {
                    //This is a small example, but demonstrates the concept of a powerful parallel lookup and process.
                    // This uses a Group Processor (group by) meaning, there can be multiple genres grouped together
                    Movie[] movies = new Movie[]
                    {
                        new Movie { Genre = "Animation", Title = "Toy Story" },
                        new Movie { Genre = "Animation", Title = "Finding Nemo" },
                        new Movie { Genre = "Adventure", Title = "The Lion King" },
                        new Movie { Genre = "Adventure", Title = "Frozen" },
                        new Movie { Genre = "Comedy", Title = "Despicable Me" }
                    };

                    // Perform parallel process and lookup
                    var genreLookup = movies.ParallelProcessToGroupProcessor(m => m.Genre);

                    //Log the genres, and their titles
                    foreach (var key in genreLookup.AllKeys())
                    {
                        l.LogInformation("Genre: {genre}, Titles: {@titles}", key, genreLookup[key].Select(f => f.Title).ToList());
                    }
                    
                    //You can check for existance of keys
                    l.LogInformation("Does the genre lookup contain comedy? {comedy}", genreLookup.Contains("Comedy") ? "Yes" : "No");
                    
                    
                    
                    //Another example, this time using the single processor. Which doesn't allow for duplicates (not a group by)
                    //Perform the parallel process
                    var spTitleLookup = movies.ParallelProcessToSingleProcessor(f => f.Title);

                    //Let's use the contains and sub-index specifier to retrieve an item.
                    if (spTitleLookup.Contains("Frozen")) {

                        //Internally items are stored in hashes, meaning retrieval is very fast
                        Movie item = spTitleLookup["Frozen"];
                        l.LogInformation("Movie: {@movie}", item);
                    }
                    
                    
                    
                    //We've seen how to use parallel to perform very fast and efficient parallel processing of lists
                    //  We can use those processed lists for very efficient iteration and lookup.
                    //  Let's iterate the list again and produce a new concurrent safe object
                    var ccBag = spTitleLookup.ParallelProcessToBag((movie) =>
                    {
                        return $"Genre: {movie.Genre};Title:{movie.Title};";
                    });

                    //Log them all out
                    foreach (var sMovie in ccBag)
                        l.LogInformation("New movie string: {movie}", sMovie);
                    
                    /*
                        Parallel processors work on lists, arrays, enumerables, and even datatables for efficient row processing
                        You can further process data by producing new data sets with the additional processes
                        You can use processed sets in hashed lookups for incredible performance boosts
                        
                        In one of our example projects we saw an 500,000% performance improvement in the code by using the parallel
                         processing libraries with their lookups on a multiple tiered conversion process
                        
                    */ 

                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                });
                
            });
            
        }
        
        public class Movie
        {
            public string Genre { get; set; }
            public string Title { get; set; }
        }

    }
}