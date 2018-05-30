using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;

using Newtonsoft.Json;

using global::Iot.Common;
using global::Iot.Common.REST;

using TargetSolution;
using Launchpad.Iot.PSG.Model;

namespace Launchpad.Iot.EventsProcessor.ExtenderService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class ExtenderService : StatelessService
    {
        private string ServiceUniqueId = FnvHash.GetUniqueId();
        private FabricClient fabricClient = new FabricClient();

        public ExtenderService(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        // The idea is to create a listening port for each instance 
                        // This application will never be called - the only purpose of this listener is

                        url += $"/eventsprocessor/extender/{ServiceUniqueId}";

                        ServiceEventSource.Current.Message( "Extender Service Initialized on " + url + " - Dummy url not to be used" );

                        return new WebHostBuilder()
                                    //.UseKestrel()
                                    .UseWebListener()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(serviceContext)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext)))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseApplicationInsights()
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Get the IoT Hub connection string from the Settings.xml config file
            // from a configuration package named "Config"
            string PublishDataServiceURLs =
                this.Context.CodePackageActivationContext
                    .GetConfigurationPackageObject("Config")
                    .Settings
                    .Sections["ExtenderServiceConfigInformation"]
                    .Parameters["PublishDataServiceURLs"]
                    .Value.Trim('/');

            ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - Starting service  - Data Service URLs[{PublishDataServiceURLs}]");

            DateTimeOffset currentSearchStartingTime = DateTimeOffset.UtcNow.AddHours(-1);

            if(PublishDataServiceURLs != null && PublishDataServiceURLs.Length > 0 )
            {
                string[] routingparts = PublishDataServiceURLs.Split(';');
                int currentValueForIntervalEnd = global::Iot.Common.Names.ExtenderStandardRetryWaitIntervalsInMills ;

                using (HttpClient httpClient = new HttpClient(new HttpServiceClientHandler()))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        string reportUniqueId = FnvHash.GetUniqueId();
                        int messageCount = 1;

                        while (messageCount > 0)
                        {
                            try
                            {
                                DateTimeOffset startTime = currentSearchStartingTime;
                                long searchIntervalStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - currentSearchStartingTime.ToUnixTimeMilliseconds() - 500; // this last adjusment is only to compensate for latency around the calls
                                long searchIntervalEnd = searchIntervalStart - currentValueForIntervalEnd;

                                if (searchIntervalEnd < 0)
                                {
                                    searchIntervalEnd = 0;
                                    currentValueForIntervalEnd = global::Iot.Common.Names.ExtenderStandardRetryWaitIntervalsInMills;
                                }

                                DateTimeOffset endTime = DateTimeOffset.UtcNow.AddMilliseconds(searchIntervalEnd * (-1));

                                string servicePathAndQuery = $"/api/devices/history/interval/{searchIntervalStart}/{searchIntervalEnd}";

                                ServiceUriBuilder uriBuilder = new ServiceUriBuilder(routingparts[0], global::Iot.Common.Names.InsightDataServiceName);
                                Uri serviceUri = uriBuilder.Build();

                                ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - About to call URL[{serviceUri}] to collect completed messages - Search[{servicePathAndQuery}] Time Start[{startTime}] End[{endTime}]");

                                // service may be partitioned.
                                // this will aggregate the queue lengths from each partition
                                System.Fabric.Query.ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

                                foreach (System.Fabric.Query.Partition partition in partitions)
                                {
                                    List<DeviceViewModelList> deviceViewModelList = new List<DeviceViewModelList>();
                                    Uri getUrl = new HttpServiceUriBuilder()
                                        .SetServiceName(serviceUri)
                                        .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                                        .SetServicePathAndQuery(servicePathAndQuery)
                                        .Build();

                                    HttpResponseMessage response = await httpClient.GetAsync(getUrl, cancellationToken);

                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        JsonSerializer serializer = new JsonSerializer();
                                        using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                                        {
                                            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                                            {
                                                List<DeviceViewModelList> resultList = serializer.Deserialize<List<DeviceViewModelList>>(jsonReader);

                                                deviceViewModelList.AddRange(resultList);
                                            }
                                        }

                                        if (deviceViewModelList.Count > 0)
                                        {
                                            DeviceViewModelList lastItem = deviceViewModelList.ElementAt(deviceViewModelList.Count()-1);

                                            messageCount = deviceViewModelList.Count;
                                            await ReportsHandler.PublishReportDataFor(reportUniqueId, routingparts[1], deviceViewModelList, this.Context, httpClient, cancellationToken, ServiceEventSource.Current,1);
                                            ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - Finished posting messages to report stream - Published total number of collected messages[{messageCount}]");
                                            currentSearchStartingTime = endTime;
                                            currentValueForIntervalEnd = global::Iot.Common.Names.ExtenderStandardRetryWaitIntervalsInMills;
                                        }
                                        else
                                        {
                                            messageCount = 0;
                                            ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - Could not find any messages in the interval from [{startTime}] to [{endTime}]");
                                        }
                                    }
                                    else
                                    {
                                        messageCount = 0;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - Severe error when reading or sending messages to report stream - Exception[{ex}] - Inner Exception[{ex.InnerException}] StackTrace[{ex.StackTrace}]");
                            }
                        }

                        currentValueForIntervalEnd += global::Iot.Common.Names.ExtenderStandardRetryWaitIntervalsInMills;

                        DateTimeOffset boundaryTime = currentSearchStartingTime.AddMilliseconds(currentValueForIntervalEnd);

                        if (boundaryTime.CompareTo(DateTimeOffset.UtcNow) > 0) 
                            await Task.Delay(global::Iot.Common.Names.ExtenderStandardRetryWaitIntervalsInMills);
                    }
                }
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"ExtenderService - {ServiceUniqueId} - RunAsync - Starting service  - Data Service URLs[{PublishDataServiceURLs}]");
            }
        }
    }
}
