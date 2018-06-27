using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Common
{
    public class MessageConfiguration
    {
        public MessageConfiguration(string messageType)
        {
            this.MessageType = messageType;

            EventRegistry.RegisterMessageType(this.MessageType, this);
        }

        public string MessageType { get; private set; }

        public DeviceMessage ManageDeviceEventSeriesContent(DeviceMessage currentSeries, DeviceMessage newSeries, out DeviceMessage completedMessage)
        {
            bool resetCurrent = true;

            if (resetCurrent)
            {
                completedMessage = new DeviceMessage(currentSeries.MessageType,currentSeries.DeviceId, currentSeries.Events);
                currentSeries = newSeries;
            }
            else
            {
                completedMessage = null;
                currentSeries.AddEvent(newSeries.Events.First());
            }

            return currentSeries;
        }
    }
}
