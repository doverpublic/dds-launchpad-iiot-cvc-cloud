// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class DeviceEvent
    {
        public DeviceEvent(DateTimeOffset timestamp, string measurementType, int sensorIndex, int tempExternal, int tempInternal, int batteryLevel, int dataPointsCount, int[] frequency, int[] magnitude )
        {
            this.Timestamp = timestamp;
            this.MeasurementType = measurementType;
            this.SensorIndex = sensorIndex;
            this.TempExternal = tempExternal;
            this.TempInternal = tempInternal;
            this.BatteryLevel = batteryLevel;
            this.DataPointsCount = dataPointsCount;
            this.Frequency = new int[frequency.Length];
            this.Magnitude = new int[magnitude.Length];

            for (int index = 0; index < frequency.Length; index++)
                this.Frequency[index] = frequency[index];

            for (int index = 0; index < magnitude.Length; index++)
                this.Magnitude[index] = magnitude[index];
        }

        [DataMember]
        public DateTimeOffset Timestamp { get; private set; }
        [DataMember]
        public string MeasurementType { get; private set; }
        [DataMember]
        public int SensorIndex { get; private set; }
        [DataMember]
        public int TempExternal { get; private set; }
        [DataMember]
        public int TempInternal { get; private set; }
        [DataMember]
        public int BatteryLevel { get; private set; }
        [DataMember]
        public int DataPointsCount { get; private set; }
        [DataMember]
        public int[] Frequency { get; protected set; }
        [DataMember]
        public int[] Magnitude { get; protected set; }

    }
}
