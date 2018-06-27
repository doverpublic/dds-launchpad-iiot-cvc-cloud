// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Controllers
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System.Fabric;
    using Microsoft.AspNetCore.Hosting;

    using Newtonsoft.Json.Linq;

    using global::Iot.Common;

    using TargetSolution;

    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private readonly IApplicationLifetime appLifetime;
        private readonly IReliableStateManager stateManager;
        private readonly StatefulServiceContext context;

        HttpClient httpClient = new HttpClient();

        public EventsController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime)
        {
            this.stateManager = stateManager;
            this.context = context;
            this.appLifetime = appLifetime;
        }

        [HttpPost]
        [Route("{deviceId}")]
        public async Task<IActionResult> Post(string deviceId )
        {
            IActionResult resultRet = this.Ok();
            DateTime durationCounter = DateTime.UtcNow;
            TimeSpan duration;
            string traceId = HashUtil.GetUniqueId();

            Stream req = Request.Body;
            string eventsArray = new StreamReader(req).ReadToEnd();

            if (String.IsNullOrEmpty(deviceId))
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.context,
                    "Data Service - Received a Really Bad Request - device id not defined");
                return this.BadRequest();
            }

            if (eventsArray == null)
            {
                ServiceEventSource.Current.ServiceMessage( this.context, $"Data Service - Received Bad Request from device {deviceId}");

                return this.BadRequest();
            }

            DeviceMessage deviceMessage = EventRegistry.DeserializeEvents(deviceId, eventsArray, this.context, ServiceEventSource.Current);

            if (deviceMessage == null)
            {
                ServiceEventSource.Current.ServiceMessage(this.context, $"Data Service - Received Bad Request from device {deviceId} - Error parsing message body [{eventsArray}]");

                return this.BadRequest();
            }

            ServiceEventSource.Current.ServiceMessage(
                                            this.context,
                                            $"Data Service - Received event from device {deviceId} for message type [{deviceMessage.MessageType}] timestamp [{deviceMessage.Timestamp}]- Traceid[{traceId}]");

            IReliableDictionary<string, DeviceMessage> storeLatestMessage = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, DeviceMessage>>(TargetSolution.Names.EventLatestDictionaryName);
            IReliableDictionary<DateTimeOffset, DeviceMessage> storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceMessage>>(TargetSolution.Names.EventHistoryDictionaryName);

            string transactionType = "";
            DeviceMessage completedMessage = null;
            DateTimeOffset messageTimestamp = DateTimeOffset.UtcNow;
            int retryCounter = 1;
            MessageConfiguration messageConfiguration = EventRegistry.GetMessageConfiguration(deviceMessage.MessageType);

            try
            {
                while (retryCounter > 0)
                {
                    transactionType = "";
                    using (ITransaction tx = this.stateManager.CreateTransaction())
                    {
                        try
                        {
                            transactionType = "In Progress Message";

                            await storeLatestMessage.AddOrUpdateAsync(
                                    tx,
                                    deviceId,
                                    deviceMessage,
                                    (key, currentValue) =>
                                    {
                                        return messageConfiguration.ManageDeviceEventSeriesContent(currentValue, deviceMessage, out completedMessage);
                                    });

                            duration = DateTime.UtcNow.Subtract(durationCounter);

                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Received event from device {deviceId} - Finished [{transactionType}] - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

                            await tx.CommitAsync();

                            retryCounter = 0;
                            duration = DateTime.UtcNow.Subtract(durationCounter);

                            if(completedMessage == null )
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Data Service - Finish commit to new partial message with timestamp [{deviceMessage.Timestamp.ToString()}] from device {deviceId} - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");
                            else
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Data Service - Finish commit to completed message with timestamp [{completedMessage.Timestamp.ToString()}] from device {deviceId} - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");
                        }
                        catch (TimeoutException tex)
                        {
                            if (global::Iot.Common.Names.TransactionsRetryCount > retryCounter)
                            {
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Message-[{tex}] - Traceid[{traceId}]");

                                await Task.Delay(global::Iot.Common.Names.TransactionRetryWaitIntervalInMills * (int)Math.Pow(2, retryCounter));
                                retryCounter++;
                            }
                            else
                            {
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Transaction Aborted - Message-[{tex}] - Traceid[{traceId}]");

                                resultRet = this.BadRequest();
                                retryCounter = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.context,
                    $"Data Service Exception when saving [{transactionType}] data from device {deviceId} - Message-[{ex}] - - Traceid[{traceId}]");
            }

            if (completedMessage != null)
            {
                transactionType = "Check Message Timestamp";
                retryCounter = 1;
                while (retryCounter > 0)
                {
                    try
                    {
                        using (ITransaction tx = this.stateManager.CreateTransaction())
                        {
                            bool tryAgain = true;
                            while (tryAgain)
                            {
                                ConditionalValue<DeviceMessage> storedCompletedMessageValue = await storeCompletedMessages.TryGetValueAsync(tx, messageTimestamp, LockMode.Default);

                                duration = DateTime.UtcNow.Subtract(durationCounter);
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Message Completed (Look for duplication - result [{storedCompletedMessageValue.HasValue}] from device {deviceId} - Starting [{transactionType}] - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

                                if (storedCompletedMessageValue.HasValue)
                                {
                                    DeviceMessage storedCompletedMessage = storedCompletedMessageValue.Value;

                                    if (completedMessage.DeviceId.Equals(storedCompletedMessage.DeviceId))
                                    {
                                        tryAgain = false; // this means this record was already saved before - no duplication necessary
                                        ServiceEventSource.Current.ServiceMessage(
                                            this.context,
                                            $"Data Service - Message with timestamp {completedMessage.Timestamp.ToString()} from device {deviceId} already present in the store - (Ignore this duplicated record) - Traceid[{traceId}]");
                                        completedMessage = null;
                                    }
                                    else
                                    {
                                        // this is a true collision between information from different devices
                                        messageTimestamp = messageTimestamp.AddMilliseconds(10);
                                        ServiceEventSource.Current.ServiceMessage(
                                            this.context,
                                            $"Data Service - Message with timestamp {completedMessage.Timestamp.ToString()} from device {deviceId} already present in the store - (Adjusted the timestamp) - Traceid[{traceId}]");
                                    }
                                }
                                else
                                {
                                    tryAgain = false;
                                }
                            }
                            await tx.CommitAsync();
                            retryCounter = 0;
                        }
                    }
                    catch (TimeoutException tex)
                    {
                        if (global::Iot.Common.Names.TransactionsRetryCount > retryCounter)
                        {
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Message-[{tex}] - Traceid[{traceId}]");

                            await Task.Delay(global::Iot.Common.Names.TransactionRetryWaitIntervalInMills * (int)Math.Pow(2, retryCounter));
                            retryCounter++;
                        }
                        else
                        {
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Transaction Aborted - Message-[{tex}] - Traceid[{traceId}]");

                            resultRet = this.BadRequest();
                            retryCounter = 0;
                        }
                    }
                }

                completedMessage.Timestamp = messageTimestamp;
                transactionType = "Save Completed Message";
                retryCounter = 1;
                while (retryCounter > 0)
                {
                    try
                    {
                        using (ITransaction tx = this.stateManager.CreateTransaction())
                        {
 
                            await storeCompletedMessages.AddOrUpdateAsync(
                                tx,
                                completedMessage.Timestamp,
                                completedMessage,
                                (key, currentValue) =>
                                {
                                    return completedMessage;
                                }
                            );

                            duration = DateTime.UtcNow.Subtract(durationCounter);
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Completed message saved message to Completed Messages Store - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");
                            await tx.CommitAsync();
                            retryCounter = 0;
                        }
                    }
                    catch (TimeoutException tex)
                    {
                        if (global::Iot.Common.Names.TransactionsRetryCount > retryCounter)
                        {
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Message-[{tex}] - Traceid[{traceId}]");

                            await Task.Delay(global::Iot.Common.Names.TransactionRetryWaitIntervalInMills * (int)Math.Pow(2, retryCounter));
                            retryCounter++;
                        }
                        else
                        {
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Timeout Exception when saving [{transactionType}] data from device {deviceId} - Iteration #{retryCounter} - Transaction Aborted - Message-[{tex}] - Traceid[{traceId}]");

                            resultRet = this.BadRequest();
                            retryCounter = 0;
                        }
                    }

                }

                duration = DateTime.UtcNow.Subtract(durationCounter);
                ServiceEventSource.Current.ServiceMessage(
                    this.context,
                    $"Data Service - Saved Message to Complete Message Store with timestamp [{completedMessage.Timestamp.ToString()}] indexed by timestamp[{messageTimestamp}] from device {deviceId} - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");
            }
        
            duration = DateTime.UtcNow.Subtract(durationCounter);
            ServiceEventSource.Current.ServiceMessage(
                this.context,
                $"Data Service Received event from device {deviceId} - Message completed Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

            return resultRet;
        }

    }
}
