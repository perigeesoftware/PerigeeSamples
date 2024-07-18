using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Perigee;

namespace Samples.CodeBlocks
{
    public static class S1_HelloWorld
    {
        public static void run()
        {

            PerigeeApplication.ApplicationNoInit("Hello World!", (c) =>
            {
                c.AddRecurring("Say Hello", (ct, l) => { l.LogInformation("I'm saying hello every 5 seconds! Press Ctrl-C to start a graceful shutdown"); }, 5000);
            });

        }
    }
}
