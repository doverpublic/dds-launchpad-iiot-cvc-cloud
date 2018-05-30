using System;
using System.Collections.Generic;
using System.IO;
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

namespace Iot.Common.REST
{

    public class RESTHandler
    {
        // read from all partititions
        public static async Task<object> ExecuteFabricGETForAllPartitions(Type targetType, string targetServiceType, string servicePathAndQuery, string entityName, ServiceContext serviceContext, HttpClient httpClient, FabricClient fabricClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            object objRet = null;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(targetServiceType);
            Uri serviceUri = uriBuilder.Build();

            // service may be partitioned.
            // this will aggregate device IDs from all partitions
            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (Partition partition in partitions)
            {
                Uri getUrl = new HttpServiceUriBuilder()
                    .SetServiceName(serviceUri)
                    .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                    .SetServicePathAndQuery(servicePathAndQuery)
                    .Build();

                HttpResponseMessage response = await httpClient.GetAsync(getUrl, cancellationToken);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    serviceEventSourceHelper.ServiceMessage(serviceContext, $"On Execute Fabric GET (For All Partitions) - Service returned result[{response.StatusCode}] for entity[{entityName}] request[{servicePathAndQuery}]");
                    objRet = response;
                }

                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        objRet = serializer.Deserialize(jsonReader, targetType);

                        if (objRet != null)
                            break;
                    }
                }
            }

            return objRet;
        }

        public static async Task<object> ExecuteFabricGETForEntity(Type targetObjectType, string targetServiceType, string servicePathAndQuery, string entityName, ServiceContext serviceContext, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            object objRet = null;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(targetServiceType);
            Uri serviceUri = uriBuilder.Build();
            long targetSiteServicePartitionKey = FnvHash.Hash(entityName);
            Uri getUrl = new HttpServiceUriBuilder()
                .SetServiceName(serviceUri)
                .SetPartitionKey(targetSiteServicePartitionKey)
                .SetServicePathAndQuery(servicePathAndQuery)
                .Build();

            HttpResponseMessage response = await httpClient.GetAsync(getUrl, cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                serviceEventSourceHelper.ServiceMessage(serviceContext, $"On Execute Fabric GET - Service returned result[{response.StatusCode}] for entity[{entityName}] request[{servicePathAndQuery}]");
                objRet = response;
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        objRet = serializer.Deserialize(jsonReader, targetObjectType);
                    }
                }
            }

            return objRet;
        }

        public static async Task<object> ExecuteFabricGET(Type targetObjectType, string targetApplicationNamePrefix, string targetSite, string targetServiceName, string servicePathAndQuery, ServiceContext serviceContext, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            object objRet = null;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(targetApplicationNamePrefix, targetSite, targetServiceName);
            Uri getUrl = new HttpServiceUriBuilder()
                .SetServiceName(uriBuilder.Build())
                .SetServicePathAndQuery(servicePathAndQuery)
                .Build();

            HttpResponseMessage response = await httpClient.GetAsync(getUrl, cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                serviceEventSourceHelper.ServiceMessage(serviceContext, $"On Execute Fabric GET - Service returned result[{response.StatusCode}] for Site[{uriBuilder.Build().ToString()}] request[{servicePathAndQuery}]");
                objRet = response;
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        objRet = serializer.Deserialize(jsonReader, targetObjectType);
                    }
                }
            }

            return objRet;
        }

        public static async Task<object> ExecuteHttpGET(Type targetObjectType, String getUrl, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            return await ExecuteHttpGET(targetObjectType, getUrl, httpClient, cancellationToken, null, serviceEventSource);
        }

        public static async Task<object> ExecuteHttpGET(Type targetObjectType, String getUrl, HttpClient httpClient, CancellationToken cancellationToken, IEnumerable<KeyValuePair<string, IEnumerable<string>>> additionalHeaders = null, IServiceEventSource serviceEventSource = null)
        {
            object objRet = null;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            if (additionalHeaders != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> item in additionalHeaders)
                {
                    if (item.Key.Equals("Authorization"))
                    {
                        string scheme = "Bearer";
                        string parameter = "";
                        int counter = 0;
                        foreach (string value in item.Value)
                        {
                            if (counter == 0)
                                scheme = value;
                            if (counter == 1)
                                parameter = value;
                            counter++;
                        }

                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                    }
                    else
                    {
                        if (item.Value.Count() > 1)
                        {
                            httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                        else
                        {
                            string value = item.Value.FirstOrDefault();

                            httpClient.DefaultRequestHeaders.Add(item.Key, value);
                        }
                    }
                }
            }

            HttpResponseMessage response = await httpClient.GetAsync(getUrl, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // This service expects the receiving target site service to return HTTP 400 if the device message was malformed.
                // In this example, the message is simply logged.
                // Your application should handle all possible error status codes from the receiving service
                // and treat the message as a "poison" message.
                // Message processing should be allowed to continue after a poison message is detected.

                string responseContent = await response.Content.ReadAsStringAsync();
                serviceEventSourceHelper.Message($"On Execute HTTP GET - Service Returned BAD REQUEST - Response Content[{responseContent}]");
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        objRet = serializer.Deserialize(jsonReader, targetObjectType);
                    }
                }
            }

            return objRet;
        }

        public static async Task<bool> ExecuteFabricPOSTForEntity(Type targetObjectType, string targetServiceType, string servicePathAndQuery, string entityName, object bodyObject, ServiceContext serviceContext, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            bool bRet = false;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            ServiceUriBuilder uriBuilder = new ServiceUriBuilder(targetServiceType);
            Uri serviceUri = uriBuilder.Build();
            long targetSiteServicePartitionKey = FnvHash.Hash(entityName);

            Uri postUrl = new HttpServiceUriBuilder()
                .SetServiceName(serviceUri)
                .SetPartitionKey(targetSiteServicePartitionKey)
                .SetServicePathAndQuery(servicePathAndQuery)
                .Build();

            string jsonStr = JsonConvert.SerializeObject(bodyObject);
            MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr));

            using (StreamContent postContent = new StreamContent(mStrm))
            {
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PostAsync(postUrl, postContent, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // This service expects the receiving target site service to return HTTP 400 if the device message was malformed.
                    // In this example, the message is simply logged.
                    // Your application should handle all possible error status codes from the receiving service
                    // and treat the message as a "poison" message.
                    // Message processing should be allowed to continue after a poison message is detected.

                    string responseContent = await response.Content.ReadAsStringAsync();

                    serviceEventSourceHelper.ServiceMessage(serviceContext, $"On Execute Fabric POST - Service returned BAD REQUEST for entity[{entityName}] request[{servicePathAndQuery}] result=[{responseContent} requestBody[{await postContent.ReadAsStringAsync()}]");
                }
                else
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        public static async Task<bool> ExecuteHttpPOST(String postUrl, object bodyObject, HttpClient httpClient, CancellationToken cancellationToken, IServiceEventSource serviceEventSource = null)
        {
            return await ExecuteHttpPOST(postUrl, bodyObject, httpClient, cancellationToken, null, serviceEventSource);
        }

        public static async Task<bool> ExecuteHttpPOST(String postUrl, object bodyObject, HttpClient httpClient, CancellationToken cancellationToken, IEnumerable<KeyValuePair<string, IEnumerable<string>>> additionalHeaders = null, IServiceEventSource serviceEventSource = null)
        {
            bool bRet = false;
            ServiceEventSourceHelper serviceEventSourceHelper = new ServiceEventSourceHelper(serviceEventSource);

            HttpContent postContent = null;

            if (bodyObject != null)
            {
                string jsonStr = JsonConvert.SerializeObject(bodyObject);

                if (jsonStr.Length > 0)
                {
                    MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr));
                    postContent = new StreamContent(mStrm);
                }
                else
                {
                    postContent = new StringContent("");
                }
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                postContent = new StringContent("");
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            if (additionalHeaders != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> item in additionalHeaders)
                {
                    if (item.Key.Equals("Authorization"))
                    {
                        string scheme = "Bearer";
                        string parameter = "";
                        int counter = 0;
                        foreach (string value in item.Value)
                        {
                            if (counter == 0)
                                scheme = value;
                            if (counter == 1)
                                parameter = value;
                            counter++;
                        }

                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                    }
                    else
                    {
                        if (item.Value.Count() > 1)
                        {
                            httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                        else
                        {
                            string value = item.Value.FirstOrDefault();

                            httpClient.DefaultRequestHeaders.Add(item.Key, value);
                        }
                    }
                }
            }

            HttpResponseMessage response = await httpClient.PostAsync(postUrl, postContent, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // This service expects the receiving target site service to return HTTP 400 if the device message was malformed.
                // In this example, the message is simply logged.
                // Your application should handle all possible error status codes from the receiving service
                // and treat the message as a "poison" message.
                // Message processing should be allowed to continue after a poison message is detected.

                string responseContent = await response.Content.ReadAsStringAsync();
                serviceEventSourceHelper.Message($"On Execute HTTP POST - Service Returned BAD REQUEST - Response Content[{responseContent}]");
            }
            else
            {
                bRet = true;
            }

            return bRet;
        }
    }
}
