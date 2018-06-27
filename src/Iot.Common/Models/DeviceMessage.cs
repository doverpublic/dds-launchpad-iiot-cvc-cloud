// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using System.Linq;

    [DataContract]
    public class DeviceMessage
    {
        public DeviceMessage()
        {
            this.MessageType = Names.EVENT_TYPE_DEFAULT;
            this.DeviceId = Names.VALUE_UNDEFINED;
            this.Events = new List<DeviceEvent>();
            this.Timestamp = DateTimeOffset.MinValue;

        }

        public DeviceMessage(string messageType, string deviceId, DeviceEvent evt )
        {
            this.MessageType = messageType;
            this.DeviceId = deviceId;
            this.Events = new List<DeviceEvent>();

            this.Events.Add(evt);

            this.Timestamp = evt.Timestamp;
        }

        public DeviceMessage(string messageType, string deviceId, IEnumerable<DeviceEvent> events)
        {
            this.MessageType = messageType;
            this.DeviceId = deviceId;
            this.Events = EventRegistry.DeepCopyEvents( events );

            DeviceEvent firstEvent = events.FirstOrDefault();

            this.Timestamp = (DateTimeOffset)firstEvent.Timestamp;
        }

        [DataMember]
        public string DeviceId { get; private set; }

        [DataMember]
        public string MessageType { get; private set; }

        [DataMember]
        public DateTimeOffset Timestamp { get; set; }

        [DataMember]
        public List<DeviceEvent> Events { get; private set; }

        
        public void AddEvent(DeviceEvent evt)
        {
            this.Events.Add(evt);
        }

        public void AddEvents(IEnumerable<DeviceEvent> events)
        {
            this.Events.AddRange(events);
        }


        // PRIVATE METHODS
    }
}
