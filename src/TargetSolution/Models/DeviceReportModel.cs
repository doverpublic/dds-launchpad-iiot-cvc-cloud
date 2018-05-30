// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


using System;

namespace Launchpad.Iot.PSG.Model
{
    public class DeviceReportModel
    {

        public DeviceReportModel(string reportUniqueId,
                                    DateTimeOffset timestamp,
                                    int timestampIndex,
                                    string deviceId,
                                    int deviceIdIndex,
                                    int batteryLevel,
                                    int batteryVoltage,
                                    int batteryMax,
                                    int batteryMin,
                                    int batteryTarget,
                                    int batteryPercentage,
                                    int batteryPercentageMax,
                                    int batteryPercentageMin,
                                    int batteryPercentageTarget,
                                    int temperatureExternal,
                                    int temperatureExternalMax,
                                    int temperatureExternalMin,
                                    int temperatureExternalTarget,
                                    int temperatureInternal,
                                    int temperatureInternalMax,
                                    int temperatureInternalMin,
                                    int temperatureInternalTarget,
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
            this.DeviceIdIndex = deviceIdIndex;
            this.BatteryLevel = batteryLevel;
            this.BatteryVoltage = batteryVoltage;
            this.BatteryMax = batteryMax;
            this.BatteryMin = batteryMin;
            this.BatteryTarget = batteryTarget;
            this.BatteryPercentage = batteryPercentage;
            this.BatteryPercentageMax = batteryPercentageMax;
            this.BatteryPercentageMin = batteryPercentageMin;
            this.BatteryPercentageTarget = batteryPercentageTarget;
            this.TemperatureExternal = temperatureExternal;
            this.TemperatureExternalMax = temperatureExternalMax;
            this.TemperatureExternalMin = temperatureExternalMin;
            this.TemperatureExternalTarget = temperatureExternalTarget;
            this.TemperatureInternal = temperatureInternal;
            this.TemperatureInternalMax = temperatureInternalMax;
            this.TemperatureInternalMin = temperatureInternalMin;
            this.TemperatureInternalTarget = temperatureInternalTarget;
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
        public int BatteryMax { get; private set; }
        public int BatteryMin { get; private set; }
        public int BatteryTarget { get; private set; }
        public int BatteryPercentage { get; private set; }
        public int BatteryPercentageMax { get; private set; }
        public int BatteryPercentageMin { get; private set; }
        public int BatteryPercentageTarget { get; private set; }
        public int TemperatureExternal { get; private set; }
        public int TemperatureExternalMax { get; private set; }
        public int TemperatureExternalMin { get; private set; }
        public int TemperatureExternalTarget { get; private set; }
        public int TemperatureInternal { get; private set; }
        public int TemperatureInternalMax { get; private set; }
        public int TemperatureInternalMin { get; private set; }
        public int TemperatureInternalTarget { get; private set; }
        public int DataPointsCount { get; private set; }
        public string MeasurementType { get; private set; }
        public int SensorIndex { get; private set; }
        public int Frequency { get; private set; }
        public int Magnitude { get; private set; }
    }
}
