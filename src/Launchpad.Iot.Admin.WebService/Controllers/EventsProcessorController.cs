// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService.Controllers
{
    using Launchpad.Iot.Admin.WebService.Models;
    using Launchpad.Iot.Admin.WebService.ViewModels;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceBus.Messaging;
    using System;
    using System.Collections.Specialized;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Fabric.Query;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Iot.Common;
    //using TargetSolution;


    [Route("api/[Controller]")]
    public class EventsProcessorController : Controller
    {
        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        private readonly FabricClient fabricClient;
        private readonly IApplicationLifetime appLifetime;

        public EventsProcessorController(FabricClient fabricClient, IApplicationLifetime appLifetime)
        {
            this.fabricClient = fabricClient;
            this.appLifetime = appLifetime;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ApplicationList applications = await this.fabricClient.QueryManager.GetApplicationListAsync();

            return this.Ok(
                applications
                    .Where(x => x.ApplicationTypeName == Names.EventsProcessorApplicationTypeName)
                    .Select(
                        x =>
                            new ApplicationViewModel(
                                x.ApplicationName.ToString(),
                                x.ApplicationStatus.ToString(),
                                x.ApplicationTypeVersion,
                                x.ApplicationParameters)));
        }

        [HttpPost]
        [Route("{name}")]
        public async Task<IActionResult> Post([FromRoute] string name, [FromBody] EventsProcessorApplicationParams parameters)
        {
            // Determine the number of IoT Hub partitions.
            // The events processing service will be created with the same number of partitions.
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(parameters.IotHubConnectionString, "messages/events");
            EventHubRuntimeInformation eventHubInfo = await eventHubClient.GetRuntimeInformationAsync();

            // Application parameters are passed to the Events Processing application instance.
            NameValueCollection appInstanceParameters = new NameValueCollection();
            appInstanceParameters["IotHubConnectionString"] = parameters.IotHubConnectionString;
            appInstanceParameters["IotHubProcessOnlyFutureEvents"] = parameters.IotHubProcessOnlyFutureEvents;
            appInstanceParameters["PublishDataServiceURLs"] = parameters.PublishDataServiceURLs;

            ApplicationDescription application = new ApplicationDescription(
                new Uri($"{Names.EventsProcessorApplicationPrefix}/{name}"),
                Names.EventsProcessorApplicationTypeName,
                parameters.Version,
                appInstanceParameters);

            // Create a named application instance
            await this.fabricClient.ApplicationManager.CreateApplicationAsync(application, this.operationTimeout, this.appLifetime.ApplicationStopping);

            // Next, create named instances of the services that run in the application.
            ServiceUriBuilder serviceNameUriBuilder = new ServiceUriBuilder(application.ApplicationName.ToString(), Names.EventsProcessorRouterServiceName);

            StatefulServiceDescription service = new StatefulServiceDescription()
            { 
                ApplicationName = application.ApplicationName,
                HasPersistedState = true,
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(eventHubInfo.PartitionCount, 0, eventHubInfo.PartitionCount - 1),
                ServiceName = serviceNameUriBuilder.Build(),
                ServiceTypeName = Names.EventsProcessorRouterServiceTypeName
            };

            await this.fabricClient.ServiceManager.CreateServiceAsync(service, this.operationTimeout, this.appLifetime.ApplicationStopping);

            if(parameters.PublishDataServiceURLs != null && parameters.PublishDataServiceURLs.Length > 0 )
            {
                // Next, create named instances of the services that run in the application.
                serviceNameUriBuilder = new ServiceUriBuilder(application.ApplicationName.ToString(), Names.EventsProcessorExtenderServiceName);

                StatelessServiceDescription extenderService = new StatelessServiceDescription()
                {
                    ApplicationName = application.ApplicationName,
                    InstanceCount = 1,
                    PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                    ServiceName = serviceNameUriBuilder.Build(),
                    ServiceTypeName = Names.EventsProcessorExtenderServiceTypeName
                };

                await this.fabricClient.ServiceManager.CreateServiceAsync(extenderService, this.operationTimeout, this.appLifetime.ApplicationStopping);
            }
            return this.Ok();
        }

        [HttpDelete]
        [Route("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                await this.fabricClient.ApplicationManager.DeleteApplicationAsync(
                    new DeleteApplicationDescription(new Uri($"{Names.EventsProcessorApplicationPrefix}/{name}")),
                    this.operationTimeout,
                    this.appLifetime.ApplicationStopping);
            }
            catch (FabricElementNotFoundException)
            { 
                // service doesn't exist; nothing to delete
            }

            return this.Ok();
        }
    }
}
