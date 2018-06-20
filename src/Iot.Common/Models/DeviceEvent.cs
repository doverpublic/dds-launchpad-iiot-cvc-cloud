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
    [KnownType("GetKnownTypes")]
    public class DeviceEvent 
    {
        public DeviceEvent()
        {
            this.EventType = Names.EVENT_TYPE_UNDEFINED;
            this.Timestamp = DateTimeOffset.MinValue;
        }

        [DataMember]
        public DateTimeOffset Timestamp { get; protected set; }

        [DataMember]
        public string EventType { get; protected set;}

        // Public Methods
        public static void RegisterDeviceEventConfigurations()
        {
            Type[] deviceEventClasses = GetKnownTypes();

            foreach(Type evtClass in deviceEventClasses)
            {
                var method = evtClass.GetMethod("RegisterDeviceEventConfigurations");
                method.Invoke(null, null);
            }
        }

        // PRiVATE METHODS
        private static Type[] GetKnownTypes()
        {
            return typeof(DeviceEvent).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(DeviceEvent))).ToArray();
        }
    }
}
