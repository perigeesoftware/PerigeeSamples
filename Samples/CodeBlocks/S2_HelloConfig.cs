using Microsoft.Extensions.Logging;
using Perigee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{
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
