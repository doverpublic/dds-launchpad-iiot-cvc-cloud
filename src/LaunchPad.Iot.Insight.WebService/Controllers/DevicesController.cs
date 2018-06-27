// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.WebService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Fabric;
    using System.Fabric.Query;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Hosting;
    using Newtonsoft.Json;

    using global::Iot.Common;
    using global::Iot.Common.Reports;

    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly IApplicationLifetime appLifetime;
        private readonly HttpClient httpClient;

        private readonly StatelessServiceContext context;
        private static NameValueCollection appSettings = ConfigurationManager.AppSettings;

        private static readonly string Username = appSettings["pbiUsername"];
        private static readonly string Password = appSettings["pbiPassword"];
        private static readonly string AuthorityUrl = appSettings["authorityUrl"];
        private static readonly string ResourceUrl = appSettings["resourceUrl"];
        private static readonly string ClientId = appSettings["clientId"];
        private static readonly string ApiUrl = appSettings["apiUrl"];
        private static readonly string GroupId = appSettings["groupId"];

        public DevicesController(FabricClient fabricClient, HttpClient httpClient, IApplicationLifetime appLifetime, StatelessServiceContext context)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.appLifetime = appLifetime;
            this.context = context;
        }


        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Devices()
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

            if (HTTPHelper.IsSessionExpired(HttpContext, this))
            {
                return Redirect(contextUri.GetServiceNameSiteHomePath());
            }
            else
            {
                this.ViewData["TargetSite"] = contextUri.GetServiceNameSite();
                this.ViewData["PageTitle"] = "Devices";
                this.ViewData["HeaderTitle"] = "Devices Dashboard";

                string reportUniqueId = HashUtil.GetUniqueId();
                string reportName = "DoverCivaconDemo"; // the dashboard

                EmbedConfig task = await ReportsHandler.GetEmbedReportConfigData(ClientId, GroupId, Username, Password, AuthorityUrl, ResourceUrl, ApiUrl, reportUniqueId, reportName, this.context, ServiceEventSource.Current);

                this.ViewData["EmbedToken"] = task.EmbedToken.Token;
                this.ViewData["EmbedURL"] = task.EmbedUrl;
                this.ViewData["EmbedId"] = task.Id;
                this.ViewData["ReportUniqueId"] = "";

                return this.View();
            }
        }

        [HttpGet]
        [Route("history/{deviceId}/byHoursInterval/{startHours}/{endHours}")]
        [Route("history/byHoursInterval/{startHours}/{endHours}")]
        [Route("history/{deviceId}/byHoursInterval/{startHours}/{endHours}/limit/{limit}")]
        [Route("history/byHoursInterval/{startHours}/{endHours}/limit/{limit}")]
        [Route("history/{deviceId}/byHoursInterval/{startHours}/{endHours}/limit/{limit}/minMagnitudeAllowed/{minMagnitudeAllowed}")]
        [Route("history/byHoursInterval/{startHours}/{endHours}/limit/{limit}/minMagnitudeAllowed/{minMagnitudeAllowed}")]
        public async Task<IActionResult> GetDevicesHistoryByInterval(int startHours, int endHours, string deviceId = null, int limit = Int32.MaxValue, int minMagnitudeAllowed = 1)
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);
            string reportsSecretKey = HTTPHelper.GetQueryParameterValueFor(HttpContext, Names.REPORTS_SECRET_KEY_NAME);
            List<DeviceMessage> deviceMessageListRet = new List<DeviceMessage>();

            long searchIntervalStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.AddHours(startHours * (-1)).ToUnixTimeMilliseconds();
            long searchIntervalEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.AddHours(endHours * (-1)).ToUnixTimeMilliseconds();

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
            Uri serviceUri = uriBuilder.Build();

            // service may be partitioned.
            // this will aggregate device IDs from all partitions
            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (Partition partition in partitions)
            {
                String pathAndQuery = $"/api/devices/history/interval/{searchIntervalStart}/{searchIntervalEnd}/limit/{limit}";

                if (deviceId != null && deviceId.Length > 0)
                    pathAndQuery = $"/api/devices/history/{deviceId}/interval/{searchIntervalStart}/{searchIntervalEnd}/limit/{limit}";

                Uri getUrl = new HttpServiceUriBuilder()
                        .SetServiceName(serviceUri)
                        .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                        .SetServicePathAndQuery(pathAndQuery)
                        .Build();

                HttpResponseMessage response = await httpClient.GetAsync(getUrl, appLifetime.ApplicationStopping);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                        {
                            List<DeviceMessage> deviceMessageListResult = serializer.Deserialize<List<DeviceMessage>>(jsonReader);

                            foreach (DeviceMessage message in deviceMessageListResult)
                            {
                                deviceMessageListRet.Add(message);
                            }
                        }
                    }
                }
            }

            return this.Ok(deviceMessageListRet);
        }

        [HttpGet]
        [Route("history/download/from/{startTimestamp}/to/{endTimestamp}")]
        [Route("history/{deviceId}/download/from/{startTimestamp}/to/{endTimestamp}")]
        public async Task<PhysicalFileResult> SearchDevicesHistoryForDownload(string startTimestamp, string endTimestamp, string deviceId = null )
        {
            string bodyPrefix = "[";
            string bodySuffix = "]";
            string bodySeparator = ",";
            bool firstElement = true;
            string fileName = Path.GetTempFileName();
            byte[] contentArray;
            int bufferSize = 4096;

            using (var fileStream = System.IO.File.Create(fileName, bufferSize))
            {
                contentArray = Encoding.ASCII.GetBytes(bodyPrefix);
                fileStream.Write(contentArray, 0, contentArray.Length);

                // Manage session and Context
                HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

                ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
                Uri serviceUri = uriBuilder.Build();

                // service may be partitioned.
                // this will aggregate the queue lengths from each partition
                ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

                foreach (Partition partition in partitions)
                {
                    bool keepLooping = true;
                    int indexStart = 0;
                    int batchSize = 200;

                    while (keepLooping)
                    {
                        string pathAndQuery = $"/api/devices/history/byKeyRange/{startTimestamp}/{endTimestamp}/{indexStart}/{batchSize}";

                        Uri getUrl = new HttpServiceUriBuilder()
                            .SetServiceName(serviceUri)
                            .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                            .SetServicePathAndQuery(pathAndQuery)
                            .Build();

                        HttpResponseMessage response = await httpClient.GetAsync(getUrl, appLifetime.ApplicationStopping);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                            {
                                using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                                {
                                    List<DeviceMessage> localResult = serializer.Deserialize<List<DeviceMessage>>(jsonReader);

                                    if (localResult != null)
                                    {
                                        if (localResult.Count > 0)
                                        {
                                            foreach (DeviceMessage device in localResult)
                                            {
                                                foreach (DeviceEvent deviceEvent in device.Events)
                                                {
                                                    if (firstElement)
                                                        firstElement = false;
                                                    else
                                                    {
                                                        contentArray = Encoding.ASCII.GetBytes(bodySeparator);
                                                        fileStream.Write(contentArray, 0, contentArray.Length);
                                                    }

                                                    string objectContent = JsonConvert.SerializeObject(deviceEvent);

                                                    contentArray = Encoding.ASCII.GetBytes(objectContent);
                                                    fileStream.Write(contentArray, 0, contentArray.Length);
                                                }
                                            }
                                        }
                                        else
                                            keepLooping = false;
                                    }
                                    else
                                    {
                                        keepLooping = false;
                                    }
                                }
                            }
                        }

                        indexStart += batchSize;
                    }
                    contentArray = Encoding.ASCII.GetBytes(bodySuffix);
                    fileStream.Write(contentArray, 0, contentArray.Length);

                    fileStream.Flush(true);

                    Response.Headers["content-disposition"] = "attachment; filename= export.json";
                    Response.ContentType = "text/json";
                }
                return PhysicalFile(fileName, "text/json", "export.json");
            }
        }


        [HttpGet]
        [Route("history/batchIndex/{batchIndex}/batchSize/{batchSize}")]
        [Route("history/batchIndex/{batchIndex}/batchSize/{batchSize}/startingAt/{startTimestamp}")]
        [Route("history/{deviceId}/batchIndex/{batchIndex}/batchSize/{batchSize}")]
        [Route("history/{deviceId}/batchIndex/{batchIndex}/batchSize/{batchSize}/startingAt/{startTimestamp}")]
        public async Task<JsonResult> SearchDevicesHistoryByPage(string deviceId = null, int batchIndex = 1, int batchSize = 200, string startTimestamp = null)
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);
            DeviceMessageList deviceMessages = new DeviceMessageList(batchIndex,batchSize);

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
            Uri serviceUri = uriBuilder.Build();

            // service may be partitioned.
            // this will aggregate the queue lengths from each partition
            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (Partition partition in partitions)
            {
                string pathAndQuery = $"/api/devices/history/batchIndex/{batchIndex}/batchSize/{batchSize}";

                if(startTimestamp != null )
                {
                    pathAndQuery = $"/api/devices/history/batchIndex/{batchIndex}/batchSize/{batchSize}/startingAt/{startTimestamp}";
                    deviceMessages.SearchStartTimestamp = DateTimeOffset.Parse(startTimestamp).ToUniversalTime();
                }

                Uri getUrl = new HttpServiceUriBuilder()
                    .SetServiceName(serviceUri)
                    .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                    .SetServicePathAndQuery(pathAndQuery)
                    .Build();

                HttpResponseMessage response = await httpClient.GetAsync(getUrl, appLifetime.ApplicationStopping);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                        {
                            DeviceMessageList resultDeviceEventSeriesList = serializer.Deserialize<DeviceMessageList>(jsonReader);
                            
                            foreach(DeviceMessage row in resultDeviceEventSeriesList.Rows)
                            {
                                deviceMessages.AddRow(row);
                            }
                            deviceMessages.TotalCount += resultDeviceEventSeriesList.TotalCount;

                            if (deviceMessages.SearchStartTimestamp.ToUnixTimeMilliseconds() < 1000)
                                deviceMessages.SearchStartTimestamp = resultDeviceEventSeriesList.SearchStartTimestamp;
                        }
                    }
                }
            }

            return this.Json(deviceMessages);
        }


        [HttpGet]
        [Route("queue/length")]
        public async Task<IActionResult> GetQueueLengthAsync()
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);
            string reportsSecretKey = HTTPHelper.GetQueryParameterValueFor(HttpContext, Names.REPORTS_SECRET_KEY_NAME);
            long count = 0;

            if((reportsSecretKey.Length == 0) && HTTPHelper.IsSessionExpired(HttpContext, this))
            {
                return Ok(contextUri.GetServiceNameSiteHomePath());
            }
            else
            {
                ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
                Uri serviceUri = uriBuilder.Build();

                // service may be partitioned.
                // this will aggregate the queue lengths from each partition
                ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

                foreach (Partition partition in partitions)
                {
                    Uri getUrl = new HttpServiceUriBuilder()
                        .SetServiceName(serviceUri)
                        .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                        .SetServicePathAndQuery($"/api/devices/queue/length")
                        .Build();

                    HttpResponseMessage response = await this.httpClient.GetAsync(getUrl, this.appLifetime.ApplicationStopping);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return this.StatusCode((int)response.StatusCode);
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    count += Int64.Parse(result);
                }
            }

            return this.Ok(count);
        }


        [HttpGet]
        [Route("device/{deviceId}")]
        [Route("deviceList")]
        public async Task<IActionResult> GetDevicesAsync( string deviceId = null )
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);
            string reportsSecretKey = HTTPHelper.GetQueryParameterValueFor(HttpContext, Names.REPORTS_SECRET_KEY_NAME);
            List<DeviceMessage> deviceMessageList = new List<DeviceMessage>();

            if ((reportsSecretKey.Length == 0) && HTTPHelper.IsSessionExpired(HttpContext, this))
            {
                return Ok(contextUri.GetServiceNameSiteHomePath());
            }
            else if (reportsSecretKey.Length > 0 )
            {
                // simply return some empty answer - no indication of error for security reasons
                if ( !reportsSecretKey.Equals(Names.REPORTS_SECRET_KEY_VALUE) )
                    return this.Ok(deviceMessageList);
            }

            deviceMessageList = await GetDevicesDataAsync( deviceId, this.httpClient, this.fabricClient, this.appLifetime);
            return this.Ok(deviceMessageList);
        }

        public static async Task<List<DeviceMessage>> GetDevicesDataAsync( string deviceId, HttpClient httpClient, FabricClient fabricClient, IApplicationLifetime appLifetime)
        {
            List<DeviceMessage> deviceEventSeriesList = new List<DeviceMessage>();
            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
            Uri serviceUri = uriBuilder.Build();

            // service may be partitioned.
            // this will aggregate device IDs from all partitions
            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (Partition partition in partitions)
            {
                Uri getUrl = new HttpServiceUriBuilder()
                    .SetServiceName(serviceUri)
                    .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                    .SetServicePathAndQuery($"/api/devices")
                    .Build();

                HttpResponseMessage response = await httpClient.GetAsync(getUrl,appLifetime.ApplicationStopping);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return deviceEventSeriesList;
                }

                JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects
                };

                string jsonObj = await response.Content.ReadAsStringAsync();
                List<DeviceMessage> result = JsonConvert.DeserializeObject<List<DeviceMessage>>(jsonObj, jsonSettings);

                if (result != null)
                {
                    if (deviceId == null)
                        deviceEventSeriesList.AddRange(result);
                    else
                    {
                        foreach (DeviceMessage deviceEventSeries in result)
                        {
                            if (deviceEventSeries.DeviceId.Equals(deviceId, StringComparison.InvariantCultureIgnoreCase))
                                deviceEventSeriesList.Add(deviceEventSeries);
                        }
                    }
                }
            }

            return deviceEventSeriesList;
        }
    }
}
