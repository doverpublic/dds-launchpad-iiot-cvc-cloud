// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    [DataContract]
    public class DeviceEventDefault : DeviceEvent
    {
        public new static void RegisterDeviceEventConfigurations()
        {
        }

        public DeviceEventDefault() : base()
        {
            this.Fields = new JObject();
        }

        public DeviceEventDefault(JObject fields) : base()
        {
            this.Timestamp = EventRegistry.GetEventTimestampFromEvent(fields);
            this.EventType = Names.EVENT_TYPE_DEFAULT;
            this.Fields = fields;
        }

        public DeviceEventDefault(string eventType, JObject fields) : base()
        {
            this.Timestamp = EventRegistry.GetEventTimestampFromEvent(fields);
            this.EventType = eventType;
            this.Fields = fields;
        }

        public DeviceEventDefault(DateTimeOffset timestamp, string eventType, JObject fields) : base()
        {
            this.Timestamp = timestamp;
            this.EventType = eventType;
            this.Fields = fields;
        }

        [DataMember]
        public JObject Fields { get; private set; }


        // PRIVATE METHODS

    }
}
