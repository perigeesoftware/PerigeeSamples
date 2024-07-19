using Microsoft.Extensions.Logging;
using Perigee;
using System;
namespace Samples.CodeBlocks
{

    /*
    ---- Hello Config ----
    - https://docs.perigee.software/getting-started/hello-configuration

    Hello Config shows how to read and deserialize the appsettings, as well as configuration linking for controlled thread starts and stops


    --== Learning Objective #1: Runtime hot-reload ==--
    * Run the application in debug mode
    * Try opening the debug folder's deployed appsettings.json (Samples\bin\Debug\net8.0\appsettings.json)
    * switch the "Enabled" flag to `true` or `false` and watch your application start or stop the TestMethod

    --> To run this sample:
    S2_HelloConfig.run();
    */
    public static class S2_HelloConfig
    {
        public static void run()
        {

            PerigeeApplication.ApplicationNoInit("HelloConfig", (taskConfig) => {

                taskConfig.AddRecurring("TestMethod", (ct, log) => {

                    //Directly by reading
                    log.LogInformation("What is my name? It is {name}", taskConfig.GetValue<string>("S2_HelloConfig:Name"));

                    //Binding a class
                    HelloConfig config = taskConfig.GetConfigurationAs<HelloConfig>("S2_HelloConfig");
                    log.LogInformation("{name} first appeared in {year:N0} and was {@tagged}", config.Name, config.Year, config.Tags);

                }, started: false).LinkToConfig("S2_HelloConfig:Enabled");
            });
        }

        public class HelloConfig
        {
            public string Name { get; set; } // HelloConfig:Name
            public int Year { get; set; } // HelloConfig:Year
            public List<string> Tags { get; set; } // HelloConfig:Tags
        }
    }
}
