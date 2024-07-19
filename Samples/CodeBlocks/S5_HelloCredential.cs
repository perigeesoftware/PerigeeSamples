using Microsoft.Extensions.Logging;
using Perigee;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samples.CodeBlocks
{

    /*
    ---- Hello Credential ----
    - https://docs.perigee.software/getting-started/hello-integration#credential-store

    This example demonstrates how to use the credential store within a thread to manage and authenticate HTTP requests.

    --== Learning Objective #1: Credential Registration ==--
    * Understand how to register a credential refresh method
    * Observe the logic for obtaining a token or credential

    --== Learning Objective #2: Using Credentials in HTTP Requests ==--
    * Learn how to create a recurring method that uses registered credentials to make HTTP requests
    * Observe the log output to see the results of the HTTP requests
    * Modify the request endpoint and authentication method to see the effects on the response

    --== Learning Objective #3: Manual Credential Retrieval ==--
    * Understand how to manually retrieve credentials and check their state
    * Observe the log output to see the credential state and expiration status
    * Modify the credential retrieval logic to include different retry and expiration settings

    --== Learning Objective #4: Application Restart and Credential Persistence ==--
    * Stop the application, and then start it again
    * Observe how the credential automatically picks back up and continues to be used in HTTP requests without having to automatically call the simulated refresh again
    * Understand the importance of credential persistence in maintaining application functionality

    --> To run this sample:
    S5_HelloCredential.run();
    */
    public static class S5_HelloCredential
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("HTTP", (c) => {

                //Register a refresh with the name "RestSharpToken".
                //  We always register credentials at the beginning a new PerigeeApplication to ensure they are ready to be used by threads
                CredentialStore.RegisterRefresh("RestSharpToken", (o) => {

                    //Add any logic you need to here, including:
                    // 1. Pulling configuration values (like so: c.GetValue<string>("AppSettings:AuthToken") )
                    // 2. Executing other authorization requests to obtain a token
                    // 3. Reaching out to another library... etc

                    c.GetLogger<Program>().LogInformation("Credential RestSharpToken is simulating a call to refresh");

                    //In this example we use a basic authentication header, many modern authentication API's now use a Bearer token:
                    //      new RestSharp.Authenticators.JwtAuthenticator("eyJ.........");
                    return new RestSharpCredentialStoreItem(new RestSharp.Authenticators.HttpBasicAuthenticator("postman", "password"),
                        Expiration: DateTimeOffset.Now.AddSeconds(3600));
                });


                //Add a method to call PMecho with credentials
                c.AddRecurring("Call Echo", (ct, l) => {

                    //Declare a client, assign the timeout to 10 seconds
                    using var client = new RestClient(new RestClientOptions("https://postman-echo.com") { MaxTimeout = 10000 });

                    //Use a CredentialAuthenticator -> pointed to the one we registered above
                    client.UseAuthenticator(new CredentialAuthenticator("RestSharpToken"));

                    //Create a request
                    var req = new RestRequest("/basic-auth", Method.Get);

                    //Execute
                    var rsp = client.Execute(req);

                    //Log
                    l.LogInformation("Postman Echo: [{code}]: {content}", (int)rsp.StatusCode, rsp.Content);

                }, 60000);

                //Manual credential retrieval
                c.AddRecurring("Manual Credentials", (ct, l) => {

                    //Get a credential, including allowing it to retry if needed
                    //      You may also supply overrides to max retries, retry time, and override the expire buffer time.
                    //      CredentialStore.GetCredential("RestSharpToken", maxRetries: 3, retryMS: 1000, expireTimeBufferSeconds: 600);
                    var credential = CredentialStore.GetCredential("RestSharpToken");

                    //Log the credential state
                    l.LogInformation("Credential state? {state}; Expires within 5 minutes?: {expired}", 
                        credential.isFaulted ? "faulted" : "NOT faulted", 
                        credential.isExpired(300) ? "is/will expire" : "will not expire");

                }, 15000);
                

            });
        }
    }
}
