using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace Iot.Common
{


    [DataContract]
    public class EdgeDevice
    {
        static EdgeDevice() { EntityRegistry.RegisterEntity(Names.EntitiesDictionaryName, "device", new EdgeDevice().GetType()); }

        public EdgeDevice()
        {
            this.Id = FnvHash.GetUniqueId();
        }

        public EdgeDevice( string deviceId )
        {
            this.Id = FnvHash.GetUniqueId();
            this.DeviceId = deviceId;
            this.DeviceName = deviceId;
        }

        public EdgeDevice(string deviceId, string deviceName)
        {
            this.Id = FnvHash.GetUniqueId();
            this.DeviceId = deviceId;
            this.DeviceName = deviceName;
            this.EventsCount = 0;
            this.MessagesCount = 0;
        }

        [DataMember]
        public string               Id { get; set; }
        [DataMember]
        public string               DeviceId { get; set; }
        [DataMember]
        public string               DeviceName { get; set; }
        [DataMember]
        public DateTimeOffset?      FirstEventTimestamp { get; private set; }
        [DataMember]
        public DateTimeOffset?      LastEventTimestamp { get; private set; }
        [DataMember]
        public int                  EventsCount { get; set; }
        [DataMember]
        public int                  MessagesCount { get; set; }


        public void AddEventCount( int count = 1 )
        {
            this.EventsCount += count;
            this.UpdateTimestamps();
        }

        public void AddMessageCount(int count = 1)
        {
            this.MessagesCount += count;
        }

        public void SubtractEventCount( int count = 1)
        {
            this.EventsCount -= count;

            if (this.EventsCount < 0)
                this.EventsCount = 0;

            if( this.EventsCount == 0 )
            {
                this.FirstEventTimestamp = null;
                this.LastEventTimestamp = null;
            }
        }


        // PRIVATE METHODS
        private void UpdateTimestamps()
        {
            DateTimeOffset now = DateTimeOffset.Now;

            if ( this.FirstEventTimestamp == null )
                this.FirstEventTimestamp = now;

            this.LastEventTimestamp = now;
        }
    }
}
