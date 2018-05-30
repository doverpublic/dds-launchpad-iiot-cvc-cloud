// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


using System;

namespace Launchpad.Iot.PSG.Model
{
    public class DeviceHistoricalReportModel
    {

        public DeviceHistoricalReportModel(string reportUniqueId,
                                    DateTimeOffset timestamp,
                                    int timestampIndex,
                                    string deviceId,
                                    int deviceIndex,
                                    int batteryLevel,
                                    int batteryVoltage,
                                    int batteryPercentage,
                                    int temperatureExternal,
                                    int temperatureInternal,
                                    int dataPointsCount,
                                    string measurementType,
                                    int sensorIndex,
                                    int frequency,
                                    int magnitude)
        {
            this.ReportUniqueId = reportUniqueId;
            this.Timestamp = timestamp;
            this.TimestampIndex = timestampIndex;
            this.DeviceId = deviceId;
            this.DeviceIdIndex = deviceIndex;
            this.BatteryLevel = batteryLevel;
            this.BatteryVoltage = batteryVoltage;
            this.BatteryPercentage = batteryPercentage;
            this.TemperatureExternal = temperatureExternal;
            this.TemperatureInternal = temperatureInternal;
            this.DataPointsCount = dataPointsCount;
            this.MeasurementType = measurementType;
            this.SensorIndex = sensorIndex;
            this.Frequency = frequency;
            this.Magnitude = magnitude;
        }

        public string ReportUniqueId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public int TimestampIndex { get; private set; }
        public string DeviceId { get; private set; }
        public int DeviceIdIndex { get; private set; }
        public int BatteryLevel { get; private set; }
        public int BatteryVoltage { get; private set; }
        public int BatteryPercentage { get; private set; }
        public int TemperatureExternal { get; private set; }
        public int TemperatureInternal { get; private set; }
        public int DataPointsCount { get; private set; }
        public string MeasurementType { get; private set; }
        public int SensorIndex { get; private set; }
        public int Frequency { get; private set; }
        public int Magnitude { get; private set; }
    }
}
