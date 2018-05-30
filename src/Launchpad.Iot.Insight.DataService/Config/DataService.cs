// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Iot.Insight.DataService.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.ServiceFabric;

    using global::Iot.Common;
    using TargetSolution;

    internal sealed class DataService : StatefulService
    {
        private const int OffloadBatchSize = TargetSolution.Names.DataOffloadBatchSize;
        private const int DrainIteration = TargetSolution.Names.DataDrainIteration;
        private readonly TimeSpan OffloadBatchInterval = TimeSpan.FromSeconds( TargetSolution.Names.DataOffloadBatchIntervalInSeconds);


        public DataService(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[1]
            {
                new ServiceReplicaListener(
                    context =>
                        new KestrelCommunicationListener(
                            context,
                            (url, listener) =>
                            {
                                ServiceEventSource.Current.Message($"Data Service Listening on {url}");
                                return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatefulServiceContext>(this.Context)
                                            .AddSingleton<IReliableStateManager>(this.StateManager)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(context)))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .UseUrls(url)
                                    .Build();
                            })
                    )
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(this.OffloadBatchInterval, cancellationToken);
            }
        }
    }
}
