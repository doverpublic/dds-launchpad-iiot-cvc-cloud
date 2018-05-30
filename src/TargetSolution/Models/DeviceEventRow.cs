using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Launchpad.Iot.PSG.Model
{
    public class DeviceEventRow
    {
        public DeviceEventRow( DateTimeOffset keyTimestamp, DateTimeOffset timestamp, string deviceId, string measurementType, int sensorIndex, int tempExternal, int tempInternal, int batteryLevel, int dataPointsCount)
        {
            this.KeyTimestamp = keyTimestamp;
            this.Timestamp = timestamp;
            this.DeviceId = deviceId;
            this.MeasurementType = measurementType;
            this.SensorIndex = sensorIndex;
            this.TemperatureExternal = tempExternal;
            this.TemperatureInternal = tempInternal;
            this.BatteryLevel = batteryLevel;
            this.DataPointsCount = dataPointsCount;
        }

        public DateTimeOffset KeyTimestamp { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string DeviceId { get; set; }
        public string MeasurementType { get; set; }
        public int SensorIndex { get; set; }
        public int TemperatureExternal { get; set; }
        public int TemperatureInternal { get; set; }
        public int BatteryLevel { get; set; }
        public int DataPointsCount { get; set; }
    }
}
