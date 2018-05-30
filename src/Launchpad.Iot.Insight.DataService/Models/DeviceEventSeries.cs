// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Models
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using System.Linq;

    [DataContract]
    internal class DeviceEventSeries
    {
        private List<DeviceEvent> EventList;

        public DeviceEventSeries(string deviceId, IEnumerable<DeviceEvent> events)
        {
            this.DeviceId = deviceId;
            this.EventList = new List<DeviceEvent>();

            foreach (DeviceEvent evnt in events)
                this.EventList.Add(new DeviceEvent(evnt.Timestamp, evnt.MeasurementType, evnt.SensorIndex, evnt.TempExternal, evnt.TempInternal, evnt.BatteryLevel, evnt.DataPointsCount, evnt.Frequency, evnt.Magnitude));

            this.Events = this.EventList;

            DeviceEvent firstEvent = events.FirstOrDefault();

            this.Timestamp = firstEvent.Timestamp;
        }


        [DataMember]
        public string DeviceId { get; private set; }

        [DataMember]
        public DateTimeOffset Timestamp { get; set; }

        [DataMember]
        public IEnumerable<DeviceEvent> Events { get; private set; }

        
        public void AddEvent(DeviceEvent evt)
        {
            this.EventList.Add(evt);
        }

        public void AddEvents(IEnumerable<DeviceEvent> events)
        {
            this.EventList.AddRange(events);
        }

    }
}
