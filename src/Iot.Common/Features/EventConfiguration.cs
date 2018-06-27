using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Common
{
    public class EventConfiguration
    {
        public EventConfiguration(string eventType, Type eventObjectType)
        {
            // this is for the case where the message has a single eventType (in this case message and event has the same type
            this.EventType = eventType;
            this.EventObectType = eventObjectType;
            this.MessageType = eventType;

            EventRegistry.RegisterEventType(this.EventType, this);
            RegisterAllEventTypes(eventType);
        }

        public EventConfiguration( string eventType, string messageType, Type eventObjectType)
        {
            this.EventType = eventType;
            this.EventObectType = eventObjectType;
            this.MessageType = messageType;

            EventRegistry.RegisterEventType(this.EventType, this);
            RegisterAllEventTypes(eventType);
        }

        public string EventType { get; private set; }

        public Type EventObectType { get; private set; }

        public string MessageType { get; private set; }


        // PRIVATE METHODS
        private void RegisterAllEventTypes( string eventType )
        {
            if( eventType.Equals(Names.EVENT_TYPE_DEFAULT))
            {
               DeviceEvent.RegisterDeviceEventConfigurations();
            }
        }
    }
}
