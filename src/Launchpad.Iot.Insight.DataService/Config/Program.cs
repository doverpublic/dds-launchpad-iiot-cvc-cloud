// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService
{
    using System.Threading;
    using System.Diagnostics;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            ServiceRuntime.RegisterServiceAsync(
                "DataServiceType",
                context =>
                    new DataService(context)).GetAwaiter().GetResult();

            ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DataService).Name);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
