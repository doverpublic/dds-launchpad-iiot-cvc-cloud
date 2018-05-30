// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.PSG.Model
{
    using System;

    public class DeviceViewModel
    {
        public DeviceViewModel(string deviceId, DateTimeOffset timestamp, string measurementType, int sensorIndex, int tempExternal, int tempInternal, int batteryLevel, int dataPointsCount, int[] frequency, int[] magnitude )
        {
            this.DeviceId = deviceId;
            this.Timestamp = timestamp;
            this.MeasurementType = measurementType;
            this.SensorIndex = sensorIndex;
            this.TempExternal = tempExternal;
            this.TempInternal = tempInternal;
            this.BatteryLevel = batteryLevel;
            this.DataPointsCount = dataPointsCount;
            this.Frequency = frequency;
            this.Magnitude = magnitude;
        }

        public string DeviceId                  { get; set; }
        public DateTimeOffset Timestamp         { get; private set; }
        public string   MeasurementType         { get; private set; }
        public int      SensorIndex             { get; private set; }
        public int      TempExternal            { get; private set; }
        public int      TempInternal            { get; private set; }
        public int      BatteryLevel            { get; private set; }
        public int      DataPointsCount         { get; private set; }
        public int[]    Frequency               { get; private set; }
        public int[]    Magnitude               { get; private set; }
    }
}
