// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Iot.Common;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.ServiceFabric;

    internal sealed class WebService : StatelessService
    {
        private readonly FabricClient fabricClient;

        public WebService(StatelessServiceContext context)
            : base(context)
        {
            this.fabricClient = new FabricClient();
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                new ServiceInstanceListener(
                    context =>
                    {
                        return new WebListenerCommunicationListener(
                            context,
                            "ServiceEndpoint",
                            (url, listener) =>
                            {
                                url += "/launchpad/iot";
                                ServiceEventSource.Current.Message($"Launchpad Admin WebService listening on {url}");
                                return new WebHostBuilder().UseWebListener()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<FabricClient>(this.fabricClient)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(context)))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .UseUrls(url)
                                    .Build();
                            });
                    })
            };
        }
    }
}
