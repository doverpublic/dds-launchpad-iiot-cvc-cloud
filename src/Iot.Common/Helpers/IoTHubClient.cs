// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO;



namespace Iot.Common
{
    public class IoTHubClient
    {

        public static async Task SendMessageToIoTHubAsync(string connectionString, IEnumerable<Device> devices, NameValueCollection keyFields, List<object> events, string messageContent = null )
        {
            string deviceId = keyFields.Get(Names.EVENT_KEY_DEVICE_ID); 
            string iotHubUri = connectionString.Split(';')
                .First(x => x.StartsWith("HostName=", StringComparison.InvariantCultureIgnoreCase))
                .Replace("HostName=", "").Trim();

            Device device = devices.FirstOrDefault(x => x.Id == deviceId);
            if (device == null)
            {
                Console.WriteLine("Device '{0}' doesn't exist.", deviceId);
            }

            DeviceClient deviceClient = DeviceClient.Create(
                iotHubUri,
                new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, device.Authentication.SymmetricKey.PrimaryKey));

            Microsoft.Azure.Devices.Client.Message message;
            JsonSerializer serializer = new JsonSerializer();
            using (MemoryStream stream = new MemoryStream())
            {
                if( messageContent == null || messageContent.Length == 0)
                {
                    using (StreamWriter streamWriter = new StreamWriter(stream))
                    {
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                        {
                            serializer.Serialize(jsonWriter, events);
                        }
                    }

                    message = new Microsoft.Azure.Devices.Client.Message(stream.GetBuffer());
                }
                else
                {
                    message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageContent));
                }

                string[] keys = keyFields.AllKeys;

                for( int index = 0; index < keyFields.Count; index++ )
                {
                    message.Properties.Add(keyFields.AllKeys[index], keyFields.Get(keyFields.AllKeys[index]) );
                }

                await deviceClient.SendEventAsync(message);

                if( messageContent != null && messageContent.Length > 0 )
                    Console.WriteLine($"Sent message: {messageContent}");
                else
                    Console.WriteLine($"Sent message: {Encoding.UTF8.GetString(stream.GetBuffer())}");
            }
        }
    }
}
