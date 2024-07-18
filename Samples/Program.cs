using Microsoft.Extensions.Logging;
using Perigee;
using Samples.CodeBlocks;

/*
 
Welcome to the perigee samples! To run any sample, please add your license file to build and select "Copy always" from the properties window. 
For a visual demo of this process, see https://docs.perigee.software/

*** Getting Started: 
    To run any individual sample, uncomment it below. They all are placed in their own class file to make inspecting the process easy.


*** Configurations:
    For samples with connection strings or configurations, they will be outlined above the sample method. Simply modify the appsettings.json file to suite your needs.   
 
 */


//Running the default sample to start, feel free to change out to other samples and read through their descriptions below!
S1_HelloWorld.run();


#region Getting Started Series

/*
    ---- Hello world ----
    - https://docs.perigee.software/

    Hello world demonstrates the basics of running a perigee application. 
     It configures the perigee start, and begins a recurring thread to log hello every 5 seconds.

    --== Learning Objective #1: Graceful Shutdown ==--
    * Start the application, then press CTRL-C on the keyboard. Watch as perigee shuts down the process while respecting the currently running threads

    --== Learning Objective #2: Change the recurring time ==--
    * Go the S1_HelloWorld.cs file
    * At the end of the .AddRecurring line, change the value from 5000 to 10000 (milliseconds) and re-run. Notice the recurring time is now 10 seconds?
    

    --> To run this sample:
    S1_HelloWorld.run();
 */





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


#endregion