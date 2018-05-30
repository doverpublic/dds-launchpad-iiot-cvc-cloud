// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Iot.Insight.DataService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System.Fabric;
    using Microsoft.AspNetCore.Hosting;

    using global::Iot.Common;
    using TargetSolution;
    using Launchpad.Iot.PSG.Model;

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
        public async Task<IActionResult> Post(string deviceId, [FromBody] IEnumerable<DeviceEvent> events)
        {
            IActionResult resultRet = this.Ok();
            DateTime durationCounter = DateTime.UtcNow;
            TimeSpan duration;
            string traceId = FnvHash.GetUniqueId();

            if (String.IsNullOrEmpty(deviceId))
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.context,
                    "Data Service - Received a Really Bad Request - device id not defined");
                return this.BadRequest();
            }

            if (events == null)
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.context,
                    $"Data Service - Received Bad Request from device {deviceId}");

                return this.BadRequest();
            }

            DeviceEvent evt = events.FirstOrDefault();

            if (evt == null)
            {
                return this.Ok();
            }

            DateTimeOffset eventTimetamp = evt.Timestamp;

            ServiceEventSource.Current.ServiceMessage(
                                            this.context,
                                            $"Data Service - Received {events.Count()} events from device {deviceId} with timestamp [{eventTimetamp}]- Traceid[{traceId}]");

            DeviceEventSeries eventList = new DeviceEventSeries(deviceId, events);

            IReliableDictionary<string, DeviceEventSeries> storeLatestMessage = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, DeviceEventSeries>>(TargetSolution.Names.EventLatestDictionaryName);
            IReliableDictionary<DateTimeOffset, DeviceEventSeries> storeCompletedMessages = await this.stateManager.GetOrAddAsync<IReliableDictionary<DateTimeOffset, DeviceEventSeries>>(TargetSolution.Names.EventHistoryDictionaryName);

            string transactionType = "";
            DeviceEventSeries completedMessage = null;
            DateTimeOffset messageTimestamp = DateTimeOffset.UtcNow;
            int retryCounter = 1;

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
                                    eventList,
                                    (key, currentValue) =>
                                    {
                                        return ManageDeviceEventSeriesContent(currentValue, eventList, out completedMessage);
                                    });

                            duration = DateTime.UtcNow.Subtract(durationCounter);
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service Received {events.Count()} events from device {deviceId} - Finished [{transactionType}] - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

                            await tx.CommitAsync();

                            retryCounter = 0;
                            duration = DateTime.UtcNow.Subtract(durationCounter);
                            ServiceEventSource.Current.ServiceMessage(
                                this.context,
                                $"Data Service - Finish commits to message with timestamp [{completedMessage.Timestamp.ToString()}] from device {deviceId} - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");
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
                                ConditionalValue<DeviceEventSeries> storedCompletedMessageValue = await storeCompletedMessages.TryGetValueAsync(tx, messageTimestamp, LockMode.Default);

                                duration = DateTime.UtcNow.Subtract(durationCounter);
                                ServiceEventSource.Current.ServiceMessage(
                                    this.context,
                                    $"Message Completed (Look for duplication - result [{storedCompletedMessageValue.HasValue}] from device {deviceId} - Starting [{transactionType}] - Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

                                if (storedCompletedMessageValue.HasValue)
                                {
                                    DeviceEventSeries storedCompletedMessage = storedCompletedMessageValue.Value;

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
                $"Data Service Received {events.Count()} events from device {deviceId} - Message completed Duration [{duration.TotalMilliseconds}] mills - Traceid[{traceId}]");

            return resultRet;
        }

        // PRIVATE METHODS
        public class TaskSynchronizationScope
        {
            private Task _currentTask;
            private readonly object _lock = new object();

            public Task RunAsync(Func<Task> task)
            {
                return RunAsync<object>(async () =>
                {
                    await task();
                    return null;
                });
            }

            public Task<T> RunAsync<T>(Func<Task<T>> task)
            {
                lock (_lock)
                {
                    if (_currentTask == null)
                    {
                        var currentTask = task();
                        _currentTask = currentTask;
                        return currentTask;
                    }
                    else
                    {
                        var source = new TaskCompletionSource<T>();
                        _currentTask.ContinueWith(t =>
                        {
                            var nextTask = task();
                            nextTask.ContinueWith(nt =>
                            {
                                if (nt.IsCompleted)
                                    source.SetResult(nt.Result);
                                else if (nt.IsFaulted)
                                    source.SetException(nt.Exception);
                                else
                                    source.SetCanceled();

                                lock (_lock)
                                {
                                    if (_currentTask.Status == TaskStatus.RanToCompletion)
                                        _currentTask = null;
                                }
                            });
                        });
                        _currentTask = source.Task;
                        return source.Task;
                    }
                }
            }
        }

        private DeviceEventSeries ManageDeviceEventSeriesContent( DeviceEventSeries currentSeries, DeviceEventSeries newSeries, out DeviceEventSeries completedMessage )
        {
            bool resetCurrent = false;

            foreach( DeviceEvent item in currentSeries.Events)
            {
                if (item.SensorIndex == newSeries.Events.First().SensorIndex )
                {
                    resetCurrent = true;
                    break;
                }
            }

            if( resetCurrent )
            {
                completedMessage = new DeviceEventSeries( currentSeries.DeviceId, currentSeries.Events );
                currentSeries = newSeries;
            }
            else
            {
                completedMessage = null;
                currentSeries.AddEvent(newSeries.Events.First());
            }

            return currentSeries;
        }
    }
}
