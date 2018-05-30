// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.WebService
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Linq;
    using System.Net.Http;

    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.ServiceFabric;

    using global::Iot.Common;

    internal sealed class WebService : StatelessService
    {
        public WebService(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                new ServiceInstanceListener(
                    context =>
                        new WebListenerCommunicationListener(
                            context,
                            "ServiceEndpoint",
                            (url, listener) =>
                            {
                                // in this sample, target site application names always have the form "fabric:/Launchpad.Iot.Insight/<TargetSiteName>
                                // This extracts the target site name from the application name and uses it as the web application path.
                                string targetSiteName = new Uri(context.CodePackageActivationContext.ApplicationName).Segments.Last();
                                url += $"/{targetSiteName}";

                                ServiceEventSource.Current.Message($"Insight Service Listening on {url}");

                                return new WebHostBuilder()
                                    .UseWebListener()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(context)
                                            .AddSingleton<FabricClient>(new FabricClient())
                                            .AddSingleton<HttpClient>(new HttpClient(new HttpServiceClientHandler()))
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(context)))
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .UseUrls(url)
                                    .Build();
                            })
                    )
            };
        }

    }
}
