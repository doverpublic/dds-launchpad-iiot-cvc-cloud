// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

   public class DeviceEntity : DynamicObject
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        // this method is relevant for the creation of the object when coming from the edge
        public DeviceEntity( string evntStr )
        {
            PopulateFields(evntStr);
        }

        // this method is relevant in case of deep copy of the DeviceEvent contents
        public DeviceEntity(DeviceEntity evnt)
        {
            // Use var keyword to enumerate dictionary.
            foreach (var property in evnt.properties)
            {
                properties.Add(property.Key, property.Value);
            }
        }

        public DateTimeOffset Timestamp {
            get {
                object objRet = null;

                if (this.properties.ContainsKey(Names.EVENT_KEY_TIMESTAMP))
                    properties.TryGetValue(Names.EVENT_KEY_TIMESTAMP, out objRet);

                return (DateTimeOffset)objRet;
            }
            private set
            {
                Timestamp = value;
            }
        }

        public string EventType {
            get
            {
                object strRet = Names.EVENT_TYPE_DEFAULT;

                if (this.properties.ContainsKey(Names.EVENT_KEY_TYPE))
                    properties.TryGetValue(Names.EVENT_KEY_TYPE, out strRet);

                return (string)strRet;
            }
            private set
            {
                EventType = value;
            }
        }

        // Public Methods
        public string[] GetFieldNames()
        {
            return this.properties.Keys.ToArray();
        }

        public object GetValueFor( string propertyName )
        {
            object objRet = null;

            if (this.properties.ContainsKey(propertyName))
                this.properties.TryGetValue(propertyName, out objRet);

            return objRet;
        }
     
        // Public Override Methods
        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember( GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name.ToLower();

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return properties.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember( SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            properties[binder.Name.ToLower()] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        // PRiVATE METHODS
        private void PopulateFields(string evntStr)
        {
            JArray fieldsArray = JArray.Parse(evntStr);

            foreach (JObject field in fieldsArray)
            {
                foreach (var pair in field)
                {
                    string value = (string)pair.Value;

                    properties.Add(pair.Key, pair.Value);
                }
            }
        }

    }
}
