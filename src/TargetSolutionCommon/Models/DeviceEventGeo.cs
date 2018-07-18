// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace TargetSolution
{
    using System;
    using System.Runtime.Serialization;

    using global::Iot.Common;

    [DataContract]
    public class DeviceEventGeo : DeviceEvent
    {
        public new static void RegisterDeviceEventConfigurations()
        {
            EventConfiguration eventConfiguration = new EventConfiguration("geolocation", typeof(DeviceEventGeo));
            MessageConfiguration messageConfiguration = new MessageConfiguration("geolocation");
        }

        public DeviceEventGeo() : base()
        {
            this.DeviceId = "";
            this.SourceName = "";
            this.Latitude = "";
            this.Longitude = "";
            this.ActivityFlag = 0;
        }

        [DataMember]
        public string DeviceId { get; private set; }
        [DataMember]
        public string SourceName { get; private set; }
        [DataMember]
        public string Latitude { get; private set; }
        [DataMember]
        public string Longitude { get; private set; }
        [DataMember]
        public int ActivityFlag { get; private set; }
    }
}
