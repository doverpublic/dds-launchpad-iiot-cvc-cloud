// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    //using System.Web.Mvc;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.AspNetCore.Hosting;

    using global::Iot.Common;
    using Newtonsoft.Json;

    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly IApplicationLifetime appLifetime;
        private readonly IReliableStateManager stateManager;
        private readonly StatefulServiceContext context;

        public DevicesController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime)
        {
            this.stateManager = stateManager;
            this.appLifetime = appLifetime;
            this.context = context;
        }


        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAsync()
        {
            List<DeviceMessage> devices = new List<DeviceMessage>();
            IReliableDictionary<string, DeviceMessage> storeLatestMessage = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, DeviceMessage>>(TargetSolution.Names.EventLatestDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                try
                {
                    IAsyncEnumerable<KeyValuePair<string, DeviceMessage>> enumerable = await storeLatestMessage.CreateEnumerableAsync(tx,EnumerationMode.Ordered);
                    IAsyncEnumerator<KeyValuePair<string, DeviceMessage>> enumerator = enumerable.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                    {
                        devices.Add(enumerator.Current.Value);
                    }
                    await tx.CommitAsync();
                }
                catch (TimeoutException te)
                {
                    // transient error. Could Retry if one desires .
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - GetAsync - TimeoutException : Message=[{te.ToString()}]");
                    tx.Abort();
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - GetAsync - General Exception - Message=[{0}]", ex);
                    tx.Abort();
                }
            }

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            return this.Ok(JsonConvert.SerializeObject(devices, jsonSettings));
        }

        [HttpGet]
        [Route("history/count/interval/{startTimestamp}/{endTimestamp}")]
        [Route("history/count/{deviceId}/interval/{startTimestamp}/{endTimestamp}")]
        public async Task<IActionResult> SearchDevicesHistoryCount(string startTimestamp, string endTimestamp = null, string deviceId = null )
        {
            int iRet = await SearchDevicesHistoryCountInternal(startTimestamp, endTimestamp, deviceId);

            return this.Ok(iRet);
        }

        [HttpGet]
        [Route("history/interval/{searchIntervalStart}/{searchIntervalEnd}")]
        [Route("history/{deviceId}/interval/{searchIntervalStart}/{searchIntervalEnd}")]
        [Route("history/interval/{searchIntervalStart}/{searchIntervalEnd}/limit/{limit}")]
        [Route("history/{deviceId}/interval/{searchIntervalStart}/{searchIntervalEnd}/limit/{limit}")]
        public async Task<IActionResult> SearchDevicesHistory( string deviceId = null, long searchIntervalStart = 86400000, long searchIntervalEnd = 0, int limit = 0)
        {
            List<DeviceMessage> deviceMessages = new List<DeviceMessage>();
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);
 
            if( storeCompletedMessages != null )
            {
                DateTimeOffset intervalToSearchStart = DateTimeOffset.UtcNow.AddMilliseconds(searchIntervalStart * (-1));
                DateTimeOffset intervalToSearchEnd = DateTimeOffset.UtcNow.AddMilliseconds(searchIntervalEnd * (-1));
                float selectInterval = 1F;

                if( limit != 0 )
                {
                    int totalCount = await SearchDevicesHistoryCountInternal(intervalToSearchStart.ToString("u"), intervalToSearchEnd.ToString("u"), deviceId);

                    if (totalCount > limit)
                        selectInterval = totalCount / limit;
                }


                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    IAsyncEnumerable<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerable = await storeCompletedMessages.CreateEnumerableAsync(
                        tx, key => (key.CompareTo(intervalToSearchStart) > 0) && (key.CompareTo(intervalToSearchEnd)<=0), EnumerationMode.Ordered);

                    IAsyncEnumerator<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerator = enumerable.GetAsyncEnumerator();

                    int index = 0;
                    int itemsAdded = 0;
                    while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                    {
                        if( index >= (itemsAdded * selectInterval))
                        {
                            itemsAdded++;

                            deviceMessages.Add(enumerator.Current.Value);
                        }
                        index++;
                    }
                    await tx.CommitAsync();
                }
            }
            return this.Ok(deviceMessages);
        }

        [HttpGet]
        [Route("history/batchIndex/{batchIndex}/batchSize/{batchSize}")]
        [Route("history/batchIndex/{batchIndex}/batchSize/{batchSize}/startingAt/{startTimestamp}")]
        [Route("history/{deviceId}/batchIndex/{batchIndex}/batchSize/{batchSize}")]
        [Route("history/{deviceId}/batchIndex/{batchIndex}/batchSize/{batchSize}/startingAt/{startTimestamp}")]
        public async Task<JsonResult> SearchDevicesHistoryByPage(string deviceId = null, int batchIndex = 1, int batchSize = 200, string startTimestamp = null)
        {
            DeviceMessageList deviceMessages = new DeviceMessageList(batchIndex,batchSize);
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);
            DateTimeOffset searchStartTimestamp = DateTimeOffset.Parse("1970-01-01T00:00:00.000Z");
            bool searchStartTimestampUpdateFlag = true;

            if (storeCompletedMessages != null)
            {
                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    IAsyncEnumerable<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerable = null;
                        
                    if(startTimestamp == null)
                    {
                        enumerable = await storeCompletedMessages.CreateEnumerableAsync(tx, EnumerationMode.Ordered);
                    }    
                    else
                    {
                        searchStartTimestamp = DateTimeOffset.Parse(startTimestamp).ToUniversalTime();
                        enumerable = await storeCompletedMessages.CreateEnumerableAsync(tx,key => key.CompareTo(searchStartTimestamp) >= 0,EnumerationMode.Ordered);
                    }

                    IAsyncEnumerator<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerator = enumerable.GetAsyncEnumerator();

                    int indexStart = batchIndex;

                    if (indexStart < 0)
                        indexStart = 0;
                    else if (indexStart > 0)
                        indexStart--;

                    indexStart = indexStart * batchSize;

                    int indexEnd = indexStart + batchSize;

                    int index = 1;
                    while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                    {
                        if (searchStartTimestampUpdateFlag)
                        {
                            searchStartTimestamp = enumerator.Current.Key;
                            searchStartTimestampUpdateFlag = false;
                        }

                        if ( deviceId == null || deviceId == enumerator.Current.Value.DeviceId)
                        {
                            if (index > indexStart && index <= indexEnd)
                            {
                                deviceMessages.AddRow( enumerator.Current.Value);
                            }
                            index++;
                        }
                    }
                    await tx.CommitAsync();
                    deviceMessages.TotalCount = index;
                    deviceMessages.SearchStartTimestamp = searchStartTimestamp;
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - SearchDevicesHistoryByPage - Count of[{deviceMessages.TotalCount}] for data range from [{startTimestamp}] - device [{deviceId ?? "All"}]");
                }
            }

            return this.Json(deviceMessages);
        }


        [HttpGet]
        [Route("history/byKey/{startTimestamp}")]
        [Route("history/byKeyRange/{startTimestamp}/{endTimestamp}")]
        [Route("history/byKeyRange/{startTimestamp}/{endTimestamp}/{indexStart}/{batchSize}")]
        [Route("history/{deviceId}/byKey/{startTimestamp}")]
        [Route("history/{deviceId}/byKeyRange/{startTimestamp}/{endTimestamp}")]
        [Route("history/{deviceId}/byKeyRange/{startTimestamp}/{endTimestamp}/{indexStart}/{batchSize}")]
        public async Task<IActionResult> SearchDevicesHistoryByKeys(string startTimestamp, string endTimestamp = null, int indexStart = (-1), int batchSize = 0, string deviceId = null)
        {
            List<object> deviceMessages = new List<object>();
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);

            if (storeCompletedMessages != null)
            {
                DateTimeOffset intervalToSearchStart = DateTimeOffset.Parse(startTimestamp).ToUniversalTime();
                DateTimeOffset intervalToSearchEnd = intervalToSearchStart;

                if (endTimestamp != null )
                    intervalToSearchEnd = DateTimeOffset.Parse(endTimestamp).ToUniversalTime();

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    IAsyncEnumerable<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerable = await storeCompletedMessages.CreateEnumerableAsync(
                        tx, key => (key.CompareTo(intervalToSearchStart) >= 0) && (key.CompareTo(intervalToSearchEnd) <= 0), EnumerationMode.Ordered);

                    IAsyncEnumerator<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerator = enumerable.GetAsyncEnumerator();

                    int index = 0;
                    while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                    {
                        if(indexStart == (-1) || index >= indexStart)
                        {
                            deviceMessages.Add(
                                new
                                {
                                    DeviceId = enumerator.Current.Value.DeviceId,
                                    enumerator.Current.Value.Events
                                });
                        }

                        if ( indexStart != (-1))
                        {
                            if (index == (indexStart + batchSize))
                                break;
                        }

                        index++;
                    }
                    await tx.CommitAsync();
                }
            }

            return this.Ok(deviceMessages);
        }

        [HttpGet]
        [Route("queue/length")]
        public async Task<IActionResult> GetQueueLengthAsync()
        {
            long count = -1;
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                try
                {
                    count = await storeCompletedMessages.GetCountAsync(tx);
                    await tx.CommitAsync();
                }
                catch (TimeoutException te)
                {
                    // transient error. Could Retry if one desires .
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - GetQueueLengthAsync - TimeoutException : Message=[{te.ToString()}]");
                    tx.Abort();
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - GetQueueLengthAsync - General Exception - Message=[{0}]", ex);
                    tx.Abort();
                }
            }

            return this.Ok(count);
        }

        // PRIVATE Methods
        private async Task<int> SearchDevicesHistoryCountInternal(string startTimestamp, string endTimestamp = null, string deviceId = null)
        {
            int iRet = 0;
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);

            if (storeCompletedMessages != null)
            {
                DateTimeOffset intervalToSearchStart = DateTimeOffset.Parse(startTimestamp).ToUniversalTime();
                DateTimeOffset intervalToSearchEnd = DateTimeOffset.UtcNow;


                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    IAsyncEnumerable<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerable = null;

                    if (endTimestamp == null)
                    {
                        enumerable = await storeCompletedMessages.CreateEnumerableAsync(
                                        tx, key => (key.CompareTo(intervalToSearchStart) >= 0), EnumerationMode.Ordered);
                    }
                    else
                    {
                        intervalToSearchEnd = DateTimeOffset.Parse(endTimestamp);
                        enumerable = await storeCompletedMessages.CreateEnumerableAsync(
                                        tx, key => (key.CompareTo(intervalToSearchStart) >= 0) && (key.CompareTo(intervalToSearchEnd) <= 0), EnumerationMode.Ordered);
                    }

                    IAsyncEnumerator<KeyValuePair<DateTimeOffset, DeviceMessage>> enumerator = enumerable.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                    {
                        iRet++;
                    }
                    await tx.CommitAsync();
                }
            }
            ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - SearchDevicesHistoryCount - Count of[{iRet}] for data range from [{startTimestamp}] to [{endTimestamp ?? "empty"}] - device [{deviceId ?? "All"}]");

            return iRet;
        }

        public class StateManagerHelper<TKeyType,TValueType> where TKeyType : IEquatable<TKeyType> , IComparable<TKeyType>
        {
            public static async Task<List<KeyValuePair<TKeyType, TValueType>>> GetAllObjectsFromStateManagerFor(StatefulServiceContext context,ITransaction tx, IReliableStateManager stateManager, string dictionaryName, IApplicationLifetime appLifetime)
            {
                List< KeyValuePair <TKeyType, TValueType >> listRet = new List<KeyValuePair<TKeyType, TValueType>>();
                
                IReliableDictionary <TKeyType, TValueType> dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<TKeyType, TValueType>>(dictionaryName);

                using (tx)
                {
                    try
                    {
                        IAsyncEnumerable<KeyValuePair<TKeyType, TValueType>> enumerable = await dictionary.CreateEnumerableAsync(tx);
                        IAsyncEnumerator<KeyValuePair<TKeyType, TValueType>> enumerator = enumerable.GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(appLifetime.ApplicationStopping))
                        {
                            if (enumerator.Current.Value.GetType() == typeof(TValueType))
                            {
                                listRet.Add(new KeyValuePair<TKeyType, TValueType>(enumerator.Current.Key, (TValueType)enumerator.Current.Value));
                            }

                        }
                    }
                    catch (TimeoutException te)
                    {
                        // transient error. Could Retry if one desires .
                        ServiceEventSource.Current.ServiceMessage( context, $"DataService - GetAllObjectsFromStateManagerFor - TimeoutException : Message=[{te.ToString()}]");
                    }
                    catch (Exception ex)
                    {
                        ServiceEventSource.Current.ServiceMessage( context, $"DataService - GetAllObjectsFromStateManagerFor - General Exception - Message=[{0}]", ex);
                        tx.Abort();
                    }

                    return listRet;
                }
            }

        }
    }
}
