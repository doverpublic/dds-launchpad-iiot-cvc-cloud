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

            if (EventRegistry.IsEventTypeAlreadyRegistered(this.EventType))
                EventRegistry.UnRegisterEventType(this.EventType);
            EventRegistry.RegisterEventType(this.EventType, this);
        }

        public EventConfiguration( string eventType, string messageType, Type eventObjectType)
        {
            this.EventType = eventType;
            this.EventObectType = eventObjectType;
            this.MessageType = messageType;

            if (EventRegistry.IsEventTypeAlreadyRegistered(this.EventType))
                EventRegistry.UnRegisterEventType(this.EventType);
            EventRegistry.RegisterEventType(this.EventType, this);
        }

        public string EventType { get; private set; }

        public Type EventObectType { get; private set; }

        public string MessageType { get; private set; }
    }
}
