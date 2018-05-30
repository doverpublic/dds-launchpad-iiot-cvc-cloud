using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Fabric;
using System.Fabric.Query;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;

using global::Iot.Common;
using global::Iot.Common.REST;


namespace Launchpad.Iot.PSG.Model
{
    public class ReportsHandler
    {
        public static async Task<EmbedConfig> GetEmbedReportConfigData(string clientId, string groupId, string username, string password, string authorityUrl, string resourceUrl, string apiUrl, string reportUniqueId, string reportName, ServiceContext serviceContext, IServiceEventSource serviceEventSource)
        {
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);
            var result = new EmbedConfig();
            var roles = "";

            try
            {
                var error = GetWebConfigErrors( clientId, groupId, username, password );
                if (error != null)
                {
                    result.ErrorMessage = error;
                    return result;
                }

                // Create a user password cradentials.
                var credential = new UserPasswordCredential(username, password);

                // Authenticate using created credentials
                var authenticationContext = new AuthenticationContext(authorityUrl);
                var authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, clientId, credential);

                if (authenticationResult == null)
                {
                    result.ErrorMessage = "Authentication Failed.";
                    return result;
                }

                var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(apiUrl), tokenCredentials))
                {
                    // Get a list of reports.
                    var reports = await client.Reports.GetReportsInGroupAsync(groupId);

                    Report report;
                    if (string.IsNullOrEmpty(reportName))
                    {
                        // Get the first report in the group.
                        report = reports.Value.FirstOrDefault();
                    }
                    else
                    {
                        report = reports.Value.FirstOrDefault(r => r.Name.Equals(reportName));
                    }

                    if (report == null)
                    {
                        result.ErrorMessage = $"PowerBI Group has no report registered for Name[{reportName}].";
                        return result;
                    }

                    var datasets = await client.Datasets.GetDatasetByIdInGroupAsync(groupId, report.DatasetId);

                    result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                    result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                    GenerateTokenRequest generateTokenRequestParameters;

                    // This is how you create embed token with effective identities
                    if( (result.IsEffectiveIdentityRequired != null && result.IsEffectiveIdentityRequired == true) &&
                        (result.IsEffectiveIdentityRolesRequired != null && result.IsEffectiveIdentityRolesRequired == true) && 
                        !string.IsNullOrEmpty(username) )
                    {
                        var rls = new EffectiveIdentity(username, new List<string> { report.DatasetId });
                        if (!string.IsNullOrWhiteSpace(roles))
                        {
                            var rolesList = new List<string>();
                            rolesList.AddRange(roles.Split(','));
                            rls.Roles = rolesList;
                        }
                        // Generate Embed Token with effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view", identities: new List<EffectiveIdentity> { rls });
                    }
                    else
                    {
                        // Generate Embed Token for reports without effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    }

                    var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id, generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        serviceEventSourceHelper.ServiceMessage(serviceContext, $"Embed Report - Error during user authentication for report - Result=[{tokenResponse.ToString()}]");
                        result.ErrorMessage = "Failed to authenticate user for report request";
                        return result;
                    }

                    // Generate Embed Configuration.
                    result.EmbedToken = tokenResponse;
                    result.EmbedUrl = report.EmbedUrl;
                    result.Id = report.Id;
                    return result;
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format($"Status: {exc.Response.StatusCode} Response Content: [{exc.Response.Content}] RequestId: {exc.Response.Headers["RequestId"].FirstOrDefault()}");
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }

            return result;
        }

        public static async Task<bool> PublishReportDataFor(string reportUniqueId, string publishUrl, List<DeviceViewModelList> deviceViewModelList, ServiceContext serviceContext, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource, int resampleSetsLimit = 0, int minMagnitudeAllowed = 1)
        {
            bool bRet = false;
            int maxSendingSet = 9999;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor - About to publish data for report id[{reportUniqueId}]  Number of messages [{deviceViewModelList.Count}] to URL [{publishUrl}]");

            if (deviceViewModelList.Count > 0)
            {
                DateTimeOffset timestamp = DateTimeOffset.UtcNow; ;
                int messageCounter = 0;
                List<DeviceReportModel> messages = new List<DeviceReportModel>();
                List<string> deviceList = new List<string>();
                int deviceIdIndex = 0;
                List<DateTimeOffset> timestampList = new List<DateTimeOffset>();
                int timestampIndex = 0;

                foreach (DeviceViewModelList deviceModel in deviceViewModelList)
                {
                    if (deviceList.Contains(deviceModel.DeviceId))
                        deviceIdIndex = deviceList.IndexOf(deviceModel.DeviceId);
                    else
                    {
                        deviceList.Add(deviceModel.DeviceId);
                        deviceIdIndex = deviceList.IndexOf(deviceModel.DeviceId);
                    }

                    bool firstItem = true;
                    string devId = deviceModel.DeviceId;
                    IEnumerable<DeviceViewModel> evts = deviceModel.Events;
                    int batteryLevel = 3300;
                    int batteryVoltage = 0;
                    int batteryMax = 4;
                    int batteryMin = 2;
                    int batteryTarget = 3;
                    int batteryPercentage = 30;
                    int batteryPercentageMax = 100;
                    int batteryPercentageMin = 0;
                    int batteryPercentageTarget = 15;
                    int temperatureExternal = 0;
                    int temperatureExternalMax = 200;
                    int temperatureExternalMin = -50;
                    int temperatureExternalTarget = 60;
                    int temperatureInternal = 0;
                    int temperatureInternalMax = 200;
                    int temperatureInternalMin = -50;
                    int temperatureInternalTarget = 60;
                    int dataPointsCount = 0;
                    string measurementType = "";
                    int sensorIndex = 0;
                    int frequency = 0;
                    int magnitude = 0;
                    bool needReferenceEntry = true;
                    int minFrequancyAllowed = 0;

                    foreach (DeviceViewModel sensorMessage in evts)
                    {
                        if (firstItem)
                        {
                            batteryLevel = sensorMessage.BatteryLevel;
                            batteryVoltage = batteryLevel / 1000;

                            if (batteryLevel < 2800)
                                batteryPercentage = 0;
                            else if (batteryLevel > 3600)
                                batteryPercentage = 100;
                            else
                                batteryPercentage = (batteryLevel - 2800) / 10;

                            timestamp = sensorMessage.Timestamp;
                            measurementType = sensorMessage.MeasurementType;
                            dataPointsCount = sensorMessage.DataPointsCount;
                            sensorIndex = sensorMessage.SensorIndex;
                            temperatureExternal = sensorMessage.TempExternal;
                            temperatureInternal = sensorMessage.TempInternal;

                            firstItem = false;

                            if (timestampList.Contains(timestamp))
                                timestampIndex = timestampList.IndexOf(timestamp);
                            else
                            {
                                timestampList.Add(timestamp);
                                timestampIndex = timestampList.IndexOf(timestamp);
                            }
                        }

                        for (int index = 0; index < sensorMessage.Frequency.Length; index++)
                        {
                            frequency = sensorMessage.Frequency[index];
                            magnitude = sensorMessage.Magnitude[index];

                            if (minFrequancyAllowed == 0)
                                minFrequancyAllowed = frequency;

                            if (magnitude >= minMagnitudeAllowed)
                            {
                                needReferenceEntry = false;

                                messages.Add(new DeviceReportModel(reportUniqueId,
                                        timestamp,
                                        timestampIndex,
                                        devId,
                                        deviceIdIndex,
                                        batteryLevel,
                                        batteryVoltage,
                                        batteryMax,
                                        batteryMin,
                                        batteryTarget,
                                        batteryPercentage,
                                        batteryPercentageMax,
                                        batteryPercentageMin,
                                        batteryPercentageTarget,
                                        temperatureExternal,
                                        temperatureExternalMax,
                                        temperatureExternalMin,
                                        temperatureExternalTarget,
                                        temperatureInternal,
                                        temperatureInternalMax,
                                        temperatureInternalMin,
                                        temperatureInternalTarget,
                                        dataPointsCount,
                                        measurementType,
                                        sensorIndex,
                                        frequency,
                                        magnitude)
                                 );
                                messageCounter++;
                            }
                        }

                        if (needReferenceEntry)
                        {
                            messages.Add(new DeviceReportModel(reportUniqueId,
                                    timestamp,
                                    timestampIndex,
                                    devId,
                                    deviceIdIndex,
                                    batteryLevel,
                                    batteryVoltage,
                                    batteryMax,
                                    batteryMin,
                                    batteryTarget,
                                    batteryPercentage,
                                    batteryPercentageMax,
                                    batteryPercentageMin,
                                    batteryPercentageTarget,
                                    temperatureExternal,
                                    temperatureExternalMax,
                                    temperatureExternalMin,
                                    temperatureExternalTarget,
                                    temperatureInternal,
                                    temperatureInternalMax,
                                    temperatureInternalMin,
                                    temperatureInternalTarget,
                                    dataPointsCount,
                                    measurementType,
                                    sensorIndex,
                                    minFrequancyAllowed,
                                    minMagnitudeAllowed)
                             );
                            messageCounter++;
                        }
                    }
                }

                serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor - Report id[{reportUniqueId}]  Total number of rows [{messageCounter}] generated from messages [{deviceViewModelList.Count}] requested for URL [{publishUrl}]");

                if (messageCounter > 0 && resampleSetsLimit > 0)
                {
                    int maxNumberOfMessagesToSend = (resampleSetsLimit * maxSendingSet);
                    List<DeviceReportModel> messagesResampledSet = new List<DeviceReportModel>();

                    if (messageCounter > maxNumberOfMessagesToSend)
                    {
                        float selectInterval = messageCounter / maxNumberOfMessagesToSend;
                        int selectedCount = 0;
                        int index = 0;
                        foreach (DeviceReportModel message in messages)
                        {
                            if (index >= (selectedCount * selectInterval))
                            {
                                selectedCount++;
                                messagesResampledSet.Add(message);
                            }
                            index++;

                            if (selectedCount == maxNumberOfMessagesToSend)
                                break;
                        }
                        messages = messagesResampledSet;
                    }
                }

                serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor - Report id[{reportUniqueId}]  Final number of rows [{messages.Count}] generated from messages [{deviceViewModelList.Count}] requested for URL [{publishUrl}]");
                if (messages.Count > 0)
                {
                    List<DeviceReportModel> messagesFinalSet = new List<DeviceReportModel>();
                    messageCounter = 0;
                    int messageSet = 1;
                    bool firstSet = true;

                    foreach (DeviceReportModel message in messages)
                    {
                        messagesFinalSet.Add(message);
                        messageCounter++;

                        if (messageCounter == maxSendingSet)
                        {
                            if( !firstSet )
                                await Task.Delay(global::Iot.Common.Names.IntervalBetweenReportStreamingCalls);

                            await RESTHandler.ExecuteHttpPOST(publishUrl, messages, httpClient, cancellationToken, serviceEventSource);
                            serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor -  Sending set [{messageSet}] with number of rows [{messageCounter}] generated from messages [{deviceViewModelList.Count}] to URL [{publishUrl}]");

                            messagesFinalSet.Clear();
                            messageCounter = 0;
                            messageSet++;
                            firstSet = false;
                        }
                    }

                    if (messagesFinalSet.Count > 0)
                    {
                        if (!firstSet)
                            await Task.Delay(global::Iot.Common.Names.IntervalBetweenReportStreamingCalls);

                        await RESTHandler.ExecuteHttpPOST(publishUrl, messages, httpClient, cancellationToken, serviceEventSource);
                        serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor -  Sending set [{messageSet}] with number of rows [{messageCounter}] generated from messages [{deviceViewModelList.Count}] to URL [{publishUrl}]");
                    }
                }

                bRet = true;
            }
            else
            {
                serviceEventSourceHelper.ServiceMessage(serviceContext, $"Embed Report - No data found to publish to [{publishUrl}]");
            }

            return bRet;
        }

        // PRIVATE METHODS

        private static string GetWebConfigErrors(string ClientId, string GroupId, string Username, string Password)
        {
            // Client Id must have a value.
            if (string.IsNullOrEmpty(ClientId))
            {
                return "ClientId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Client Id must be a Guid object.
            if (!Guid.TryParse(ClientId, out Guid result))
            {
                return "ClientId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Group Id must have a value.
            if (string.IsNullOrEmpty(GroupId))
            {
                return "GroupId is empty. Please select a group you own and fill its Id in web.config";
            }

            // Group Id must be a Guid object.
            if (!Guid.TryParse(GroupId, out result))
            {
                return "GroupId must be a Guid object. Please select a group you own and fill its Id in web.config";
            }

            // Username must have a value.
            if (string.IsNullOrEmpty(Username))
            {
                return "Username is empty. Please fill Power BI username in web.config";
            }

            // Password must have a value.
            if (string.IsNullOrEmpty(Password))
            {
                return "Password is empty. Please fill password of Power BI username in web.config";
            }

            return null;
        }
    }
}
