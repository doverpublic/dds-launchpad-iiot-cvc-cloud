using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net.Http;
using System.Fabric;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;

using global::Iot.Common;
using global::Iot.Common.REST;


namespace Iot.Common.Reports
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

        public static async Task<bool> PublishReportDataFor(string reportUniqueId, string datasetName, List<DeviceMessage> deviceMessagelList, ServiceContext serviceContext, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource, int resampleSetsLimit = 0, int minMagnitudeAllowed = 1)
        {
            bool bRet = false;
            int maxSendingSet = 9999;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);
            DatasetConfiguration datasetConfiguration = DatasetRegistry.GetDatasetConfiguration(datasetName);
            int initialMessageCount = 0;

            serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor - About to publish data for Report id[{reportUniqueId}] Dataset Name[{datasetName}] Number of messages [{deviceMessagelList.Count}]");

            ReportStreamDataset reportStreamDataset = new ReportStreamDataset(reportUniqueId, datasetConfiguration, deviceMessagelList);

            if (reportStreamDataset.Count > 0 && resampleSetsLimit > 0)
            {
                initialMessageCount = reportStreamDataset.Count;
                int maxNumberOfMessagesToSend = (resampleSetsLimit * maxSendingSet);
                ReportStreamDataset resampledReportStreamDataset = new ReportStreamDataset(reportUniqueId, datasetConfiguration);

                if (reportStreamDataset.Count > maxNumberOfMessagesToSend)
                {
                    float selectInterval = reportStreamDataset.Count / maxNumberOfMessagesToSend;
                    int selectedCount = 0;
                    int index = 0;

                    foreach (DeviceMessage message in reportStreamDataset.DeviceMessageList)
                    {
                        if (index >= (selectedCount * selectInterval))
                        {
                            selectedCount++;
                            resampledReportStreamDataset.AddMessage(message);
                        }
                        index++;

                        if (selectedCount == maxNumberOfMessagesToSend)
                            break;
                    }
                    reportStreamDataset = resampledReportStreamDataset;
                }
            }

            serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor - Report id[{reportUniqueId}]  Final number of rows [{reportStreamDataset.Count}] generated from messages [{initialMessageCount}] requested for Dataset [{datasetName}]");

            if (reportStreamDataset.Count > 0)
            {
                int messageCounter = 0;
                int messageSet = 1;
                bool firstSet = true;
                StringBuilder bodyContent = new StringBuilder();
                bodyContent.Append("[");
                bool firstItem = true;

                foreach (DeviceMessage message in reportStreamDataset.DeviceMessageList)
                {
                    if (firstItem)
                        firstItem = false;
                    else
                        bodyContent.Append(",");

                    bodyContent.Append(datasetConfiguration.GetMessageToPublish(message));
                    messageCounter++;

                    if (messageCounter == maxSendingSet)
                    {
                        if (!firstSet)
                            await Task.Delay(global::Iot.Common.Names.IntervalBetweenReportStreamingCalls);

                        bodyContent.Append("]");
                        await RESTHandler.ExecuteHttpPOST(datasetConfiguration.PostUrl, bodyContent.ToString(), httpClient, cancellationToken, serviceEventSource);
                        serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor -  Sending set [{messageSet}] with number of rows [{messageCounter}] generated from messages [{reportStreamDataset.Count}] to dataset [{datasetName}]");

                        bodyContent.Clear();
                        bodyContent.Append("[");
                        firstItem = true;
                        messageCounter = 0;
                        messageSet++;
                        firstSet = false;
                    }
                }

                if (reportStreamDataset.Count > 0)
                {
                    if (!firstSet)
                        await Task.Delay(global::Iot.Common.Names.IntervalBetweenReportStreamingCalls);
                    bodyContent.Append("]");
                    await RESTHandler.ExecuteHttpPOST(datasetConfiguration.PostUrl, bodyContent, httpClient, cancellationToken, serviceEventSource);
                    serviceEventSourceHelper.ServiceMessage(serviceContext, $"PublishReportDataFor -  Sending set [{messageSet}] with number of rows [{messageCounter}] generated from messages [{reportStreamDataset.Count}] to dataset [{datasetName}]");
                }
                bRet = true;
            }
            else
            {
                serviceEventSourceHelper.ServiceMessage(serviceContext, $"Embed Report - No data found to publish for Dataset [{datasetName}]");
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
