// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService.Controllers
{
    using Iot.Admin.WebService.Models;
    using Iot.Admin.WebService.ViewModels;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Fabric.Query;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Iot.Common;

    [Route("api/[Controller]")]
    public class InsightsController : Controller
    {
        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        private readonly FabricClient fabricClient;
        private readonly IApplicationLifetime appLifetime;

        public InsightsController(FabricClient fabricClient, IApplicationLifetime appLifetime)
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
                    .Where(x => x.ApplicationTypeName == Names.InsightApplicationTypeName)
                    .Select(
                        x =>
                            new ApplicationViewModel(
                                x.ApplicationName.ToString(),
                                x.ApplicationStatus.ToString(),
                                x.ApplicationTypeVersion,
                                x.ApplicationParameters)));
        }

        [HttpPost]
        [Route("{targetSiteName}")]
        public async Task<IActionResult> Post([FromRoute] string targetSiteName, [FromBody] InsightApplicationParams parameters)
        {
            // First create the application instance.
            // This won't actually create the services yet.
            ApplicationDescription application = new ApplicationDescription(
                new Uri($"{Names.InsightApplicationNamePrefix}/{targetSiteName}"),
                Names.InsightApplicationTypeName,
                parameters.Version);

            await this.fabricClient.ApplicationManager.CreateApplicationAsync(application, this.operationTimeout, this.appLifetime.ApplicationStopping);

            // Now create the data service in the new application instance.
            ServiceUriBuilder dataServiceNameUriBuilder = new ServiceUriBuilder(application.ApplicationName.ToString(), Names.InsightDataServiceName);
            StatefulServiceDescription dataServiceDescription = new StatefulServiceDescription()
            {
                ApplicationName = application.ApplicationName,
                HasPersistedState = true,
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(parameters.DataPartitionCount, Int64.MinValue, Int64.MaxValue),
                ServiceName = dataServiceNameUriBuilder.Build(),
                ServiceTypeName = Names.InsightDataServiceTypeName
            };

            await this.fabricClient.ServiceManager.CreateServiceAsync(dataServiceDescription, this.operationTimeout, this.appLifetime.ApplicationStopping);

            // And finally, create the web service in the new application instance.
            ServiceUriBuilder webServiceNameUriBuilder = new ServiceUriBuilder(application.ApplicationName.ToString(), Names.InsightWebServiceName);
            StatelessServiceDescription webServiceDescription = new StatelessServiceDescription()
            {
                ApplicationName = application.ApplicationName,
                InstanceCount = parameters.WebInstanceCount,
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                ServiceName = webServiceNameUriBuilder.Build(),
                ServiceTypeName = Names.InsightWebServiceTypeName
            };

            await this.fabricClient.ServiceManager.CreateServiceAsync(webServiceDescription, this.operationTimeout, this.appLifetime.ApplicationStopping);


            return this.Ok();
        }

        [HttpDelete]
        [Route("{targetSiteName}")]
        public async Task<IActionResult> Delete(string targetSiteName)
        {
            try
            {
                await this.fabricClient.ApplicationManager.DeleteApplicationAsync(
                    new DeleteApplicationDescription(new Uri($"{Names.InsightApplicationNamePrefix}/{targetSiteName}")),
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
