using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Fabric;

using System.Runtime.Serialization.Json;

using Newtonsoft.Json.Linq;


namespace Iot.Common
{
    public class EventRegistry
    {
        private static ConcurrentDictionary<string, EventConfiguration> eventTypesBag = new ConcurrentDictionary<string, EventConfiguration>();
        private static ConcurrentDictionary<string, MessageConfiguration> messageTypesBag = new ConcurrentDictionary<string, MessageConfiguration>();

        private static EventConfiguration defaultEventConfiguration = new EventConfiguration(Names.EVENT_TYPE_DEFAULT, typeof(DeviceEventDefault));
        private static MessageConfiguration defaultMessageConfiguration = new MessageConfiguration(Names.EVENT_TYPE_DEFAULT);

        public static List<DeviceEvent> DeepCopyEvents( IEnumerable<DeviceEvent> events )
        {
            List<DeviceEvent> evtRet = new List<DeviceEvent>();

            foreach( DeviceEvent evt in events)
            {
                JObject jsonObj = SerializeEvent(evt);

                evtRet.Add( DeserializeEvent( evt.GetType(), jsonObj) );
            }

            return evtRet;
        }

        public static DeviceMessage DeserializeEvents(string deviceId, string arrayEventStr, ServiceContext serviceContext, IServiceEventSource serviceEventSource )
        {
            DeviceMessage deviceMessageRet = null; 

            try
            {
                JArray jarr = JArray.Parse(arrayEventStr);

                deviceMessageRet = DeserializeEvents(deviceId, jarr, serviceContext, serviceEventSource);
            }
            catch (Exception ex)
            {
                serviceEventSource.ServiceMessage(serviceContext, $"Event Registry - Could not parse device id[{deviceId}] message [{arrayEventStr}] Error Message [{ex.Message}]");
            }

            return deviceMessageRet;
        }

        public static DeviceMessage DeserializeEvents( string deviceId, JArray events, ServiceContext serviceContext, IServiceEventSource serviceEventSource )
        {
            DeviceMessage messageRet = null;
            bool firstEvent = true;

            foreach( JObject evt in events)
            {
                String eventType = GetEventTypeFromEvent(evt);

                if( eventType.Equals( Names.EVENT_TYPE_DEFAULT))
                    serviceEventSource.ServiceMessage(serviceContext, $"Event Registry - Could not find event configuration for event type [{eventType}] - Will parse the event as Default Type");

                EventConfiguration eventConfiguration = EventRegistry.GetEventConfiguration(eventType);

                DeviceEvent deviceEvent = DeserializeEvent(eventConfiguration, evt);

                if( firstEvent)
                {
                    messageRet = new DeviceMessage(eventConfiguration.MessageType, deviceId, deviceEvent);
                    firstEvent = false;
                }
                else
                {
                    messageRet.AddEvent(deviceEvent);
                }
            }

            return messageRet;
        }

        public static EventConfiguration GetEventConfiguration( string eventType )
        {
            EventConfiguration eventConfigurationRet = defaultEventConfiguration;

            if (IsEventTypeAlreadyRegistered(eventType))
            {
                eventTypesBag.TryGetValue(eventType, out eventConfigurationRet);
            }

            return eventConfigurationRet;
        }

        public static MessageConfiguration GetMessageConfiguration(string messageType)
        {
            MessageConfiguration messageConfigurationRet = defaultMessageConfiguration;

            if (IsMessageTypeAlreadyRegistered(messageType))
            {
                messageTypesBag.TryGetValue(messageType, out messageConfigurationRet);
            }

            return messageConfigurationRet;
        }

        public static DateTimeOffset GetEventTimestampFromEvent(JObject evt)
        {
            DateTimeOffset timestampRet = DateTimeOffset.MinValue;

            foreach (var pair in evt)
            {
                if (pair.Key.Equals(Names.EVENT_KEY_TIMESTAMP, StringComparison.InvariantCultureIgnoreCase))
                {
                    timestampRet = DateTimeOffset.Parse((string)pair.Value);
                    break;
                }
            }

            return timestampRet;
        }

        public static string GetEventTypeFromEvent(JObject evt)
        {
            string strRet = Names.EVENT_TYPE_DEFAULT;

            foreach (var pair in evt)
            {
                if (pair.Key.Equals(Names.EVENT_KEY_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    strRet = (string)pair.Value;
                    break;
                }
            }

            return strRet;
        }

        public static List<string> GetEventTypeListForMessageType( string messageType )
        {
            List<string> listRet = new List<string>();

            foreach( var item in eventTypesBag)
            {
                EventConfiguration eventConfiguration = item.Value;

                if (eventConfiguration.Equals(messageType))
                    listRet.Add(item.Key);
            }

            return listRet;
        }

        public static Type[] GetEventTypeReferencesFromAllAssemblies()
        {
            IEnumerable<Type> types = ReflexUtil.GetKnownTypesFromAllCurrentDomainAssemblies(typeof(DeviceEvent));

            return types.ToArray<Type>();
        }

        public static bool IsEventTypeAlreadyRegistered(string eventType)
        {
            return eventTypesBag.ContainsKey(eventType);
        }

        public static bool IsMessageTypeAlreadyRegistered(string messageType)
        {
            return messageTypesBag.ContainsKey(messageType);
        }

        public static bool RegisterEventType( string eventType, EventConfiguration eventConfiguration )
        {
            bool bRet = false;

            if( !IsEventTypeAlreadyRegistered(eventType))
            {
                eventTypesBag.TryAdd(eventType, eventConfiguration);
                bRet = true;
            }

            return bRet;
        }

        public static bool RegisterMessageType(string messageType, MessageConfiguration messageConfiguration)
        {
            bool bRet = false;

            if (!IsMessageTypeAlreadyRegistered(messageType))
            {
                messageTypesBag.TryAdd(messageType, messageConfiguration);
                bRet = true;
            }

            return bRet;
        }

        public static JArray SerializeEvents(string deviceId, DeviceMessage deviceMessage, ServiceContext serviceContext, IServiceEventSource serviceEventSource)
        {
            JArray jarrRet = new JArray();

            foreach (DeviceEvent deviceEvent in deviceMessage.Events)
            {
                JObject jsonObj = SerializeEvent(deviceEvent);

                jarrRet.Add(jsonObj);
            }

            return jarrRet;
        }

        public static bool UnRegisterEventType(string eventType)
        {
            bool bRet = false;

            if (!IsEventTypeAlreadyRegistered(eventType))
            {
                eventTypesBag.TryRemove(eventType, out EventConfiguration eventConfiguration);
                bRet = true;
            }

            return bRet;
        }

        public static bool UnRegisterMessageType(string messageType)
        {
            bool bRet = false;

            if (!IsMessageTypeAlreadyRegistered(messageType))
            {
                messageTypesBag.TryRemove(messageType, out MessageConfiguration messageConfiguration);
                bRet = true;
            }

            return bRet;
        }

        //PRIVATE METHODS
        private static DeviceEvent DeserializeEvent(Type eventType, JObject jsonObj)
        {
            DeviceEvent deviceEventRet = (DeviceEvent)jsonObj.ToObject(eventType);

            if (deviceEventRet == null)
                deviceEventRet = new DeviceEventDefault(jsonObj);

            return deviceEventRet;
        }

        private static DeviceEvent DeserializeEvent(EventConfiguration eventConfiguration, JObject jsonObj)
        {
            DeviceEvent deviceEventRet = (DeviceEvent)jsonObj.ToObject(eventConfiguration.EventObectType);

            if (deviceEventRet == null)
                deviceEventRet = new DeviceEventDefault(eventConfiguration.EventType, jsonObj);

            return deviceEventRet;
        }

        private static DeviceEvent DeserializeEvent( Type deviceEventType, string jsonStr )
        {
            DeviceEvent deviceEventRet = null;

            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(jsonStr)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(deviceEventType);
                deviceEventRet = (DeviceEvent)deserializer.ReadObject(ms);
            }
            return deviceEventRet;
        }


        private static string[] GetEventTypesFromEvents(JArray events)
        {
            string[] arrRet = new string[events.Count];

            int index = 0;
            foreach (JObject evt in events)
            {
                arrRet[index] = Names.EVENT_TYPE_DEFAULT;
                foreach (var pair in evt)
                {
                    if (pair.Key.Equals(Names.EVENT_KEY_TYPE, StringComparison.InvariantCultureIgnoreCase))
                    {
                        arrRet[index] = (string)pair.Value;
                        break;
                    }
                }
                index++;
            }

            return arrRet;
        }

        private static JObject SerializeEvent( DeviceEvent deviceEvent)
        {
            JObject jsonObj = JObject.FromObject(deviceEvent);

            return jsonObj;
        }

        private static string SerializeEvent(Type deviceEventType, DeviceEvent deviceEvent)
        {
            string strRet = null;

            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(deviceEventType);

                serializer.WriteObject(ms, deviceEvent);

                strRet = Encoding.Unicode.GetString(ms.ToArray());
            }
            return strRet;
        }

    }
}
