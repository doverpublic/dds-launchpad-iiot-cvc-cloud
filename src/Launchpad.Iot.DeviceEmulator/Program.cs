// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.DeviceEmulator
{
    using System;
    using System.Fabric;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    using System.Collections.Generic;
    using System.Collections.Specialized;

    using global::Iot.Common;

    internal class Program
    {
        private static string connectionString;
        private static string clusterAddress;
        private static RegistryManager registryManager;
        private static FabricClient fabricClient;
        private static IEnumerable<Device> devices;
        private static IEnumerable<string> targetSites;

        // credential fields
        private static X509Credentials credential;
        private static string credentialType;
        private static string findType;
        private static string findValue;
        private static string serverCertThumbprint;
        private static string storeLocation;
        private static string storeName;
   

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter IoT Hub Connection String: ");
            connectionString = Console.ReadLine();

            Console.WriteLine("Enter Service Fabric cluster Address Where your IoT Solution is Deployed (or blank for local): ");
            clusterAddress = Console.ReadLine();

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);


            // let's deal with collecting credentials information for the cluster connection
            if( !String.IsNullOrEmpty(clusterAddress) )
            {
                Console.WriteLine("Enter Credential Type [none, x509, Windows] (or blank for unsecured Service Fabric): ");
                credentialType = Console.ReadLine();

                Console.WriteLine("Enter Server Certificate Thumbprint  (or blank for not working with server certificate): ");
                serverCertThumbprint = Console.ReadLine();

                if ( !String.IsNullOrEmpty(credentialType) || !String.Equals( credentialType, "none" ) )
                {
                    Console.WriteLine("Enter Credential Find Type [FindByThumbprint, ... ] (or blank for not working with find type): ");
                    findType = Console.ReadLine();
                }

                if( !String.IsNullOrEmpty(findType) )
                { 
                    Console.WriteLine("Enter Credential Find Value: ");
                    findValue = Console.ReadLine();

                    Console.WriteLine("Enter Credential Find Location: ");
                    storeLocation = Console.ReadLine();

                    Console.WriteLine("Enter Credential Find Location Name: ");
                    storeName = Console.ReadLine();
                }
            }

            if (!String.IsNullOrEmpty(findType))
            {
                credential = new X509Credentials();

                credential.RemoteCertThumbprints.Add( serverCertThumbprint );

                if( String.Equals( storeLocation.ToUpper(), "CURRENTUSER" ))
                    credential.StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser;
                else
                    credential.StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine;

                credential.StoreName = storeName;
                credential.FindValue = findValue;

                if (String.Equals(findType.ToUpper(), "FINDBYTHUMBPRINT"))
                    credential.FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint;
                else
                    Console.WriteLine("X509 Find Type Not Supported [{0}]", findType);
            }

            fabricClient = String.IsNullOrEmpty(clusterAddress)
            ? new FabricClient()
            : credential == null ? new FabricClient(clusterAddress) : new FabricClient(credential, clusterAddress);

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        try
                        {
                            devices = await registryManager.GetDevicesAsync(Int32.MaxValue);
                            targetSites = (await fabricClient.QueryManager.GetApplicationListAsync())
                                .Where(x => x.ApplicationTypeName == Names.InsightApplicationTypeName)
                                .Select(x => x.ApplicationName.ToString().Replace(Names.InsightApplicationNamePrefix + "/", ""));

                            Console.WriteLine();
                            Console.WriteLine("Devices IDs: ");
                            foreach (Device device in devices)
                            {
                                Console.WriteLine(device.Id);
                            }

                            Console.WriteLine();
                            Console.WriteLine("Insight Application URI: ");
                            foreach (string targetSiteName in targetSites)
                            {
                                Console.WriteLine(targetSiteName);
                            }

                            Console.WriteLine();
                            Console.WriteLine("Commands:");
                            Console.WriteLine("1: Register a device");
                            Console.WriteLine("2: Register random devices");
                            Console.WriteLine("3: Send data from a device");
                            Console.WriteLine("4: Send data from all devices");
                            Console.WriteLine("5: Send data from a CSV File from a device ");
                            Console.WriteLine("6: Send data from a CSV File");
                            Console.WriteLine("7: Send data from a JSON File");
                            Console.WriteLine("8: Exit");

                            string command = Console.ReadLine();
                            string deviceId = "";
                            string targetSite = "";
                            string fileDataPath = "";
                            string fieldDefinitionsPath = "";
                            string messageContent = "";

                            switch (command)
                            {
                                case "1":
                                    Console.WriteLine("Make up a Device ID: ");
                                    deviceId = Console.ReadLine();
                                    await AddDeviceAsync(deviceId);
                                    break;
                                case "2":
                                    Console.WriteLine("How many devices? ");
                                    int num = Int32.Parse(Console.ReadLine());
                                    await AddRandomDevicesAsync(num);
                                    break;
                                case "3":
                                    Console.WriteLine("Target Application URI: ");
                                    targetSite = Console.ReadLine();
                                    Console.WriteLine("Device ID: ");
                                    deviceId = Console.ReadLine();
                                    Console.WriteLine("Message Content or ENTER to create dummy messages:");
                                    messageContent = Console.ReadLine();
                                    await SendDeviceToCloudMessagesAsync(deviceId, targetSite, messageContent);
                                    break;
                                case "4":
                                    Console.WriteLine("Target Site Application URI: ");
                                    targetSite = Console.ReadLine();
                                    Console.WriteLine("Iterations: ");
                                    int iterations = Int32.Parse(Console.ReadLine());
                                    await SendAllDevices(targetSite, iterations);
                                    break;
                                case "5":
                                    Console.WriteLine("Target Site Application URI: ");
                                    targetSite = Console.ReadLine();
                                    Console.WriteLine("Device ID: ");
                                    deviceId = Console.ReadLine();
                                    Console.WriteLine("CSV File Path ([data only] or [data + fieldefinitions]): ");
                                    fileDataPath = Console.ReadLine();
                                    Console.WriteLine("CSV File Path ([fieldefinitions] or ENTER in case of [data + fieldefinitions]): ");
                                    fieldDefinitionsPath = Console.ReadLine();

                                    if(fileDataPath.Length > 0)
                                        await SendEventsFromCSVFile(deviceId, targetSite, fileDataPath, fieldDefinitionsPath);
                                    else
                                        Console.WriteLine("No valid path provided for CSV data file");
                                    break;
                                case "6":
                                    Console.WriteLine("CSV File Path ([data only] or [data + fieldefinitions]): ");
                                    fileDataPath = Console.ReadLine();
                                    Console.WriteLine("CSV File Path ([fieldefinitions] or ENTER in case of [data + fieldefinitions]): ");
                                    fieldDefinitionsPath = Console.ReadLine();

                                    if (fileDataPath.Length > 0)
                                        await SendEventsFromCSVFile(deviceId, targetSite, fileDataPath, fieldDefinitionsPath);
                                    else
                                        Console.WriteLine("No valid path provided for CSV data file");
                                    break;
                                case "7":
                                    Console.WriteLine("Target Application URI: ");
                                    targetSite = Console.ReadLine();
                                    Console.WriteLine("Device ID: ");
                                    deviceId = Console.ReadLine();
                                    Console.WriteLine("JSON File Path:");
                                    fileDataPath = Console.ReadLine();
                                    await SendEventsFromJSONFile(deviceId, targetSite, fileDataPath);
                                    break;
                                case "8":
                                    return;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Oops, {0}", ex.Message);
                        }
                    }
                })
                .GetAwaiter().GetResult();
        }

        private static async Task SendAllDevices(string targetSite, int iterations)
        {
            for (int i = 0; i < iterations; ++i)
            {
                try
                {
                    List<Task> tasks = new List<Task>(devices.Count());
                    foreach (Device device in devices)
                    {
                        tasks.Add(SendDeviceToCloudMessagesAsync(device.Id, targetSite));
                    }

                    await Task.WhenAll(tasks);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Send failed. {0}", ex.Message);
                }
            }
        }

        private static async Task SendDeviceToCloudMessagesAsync(string deviceId, string targetSite, string messageContent = null)
        {
            List<object> events = new List<object>();

            if( messageContent == null || messageContent.Length == 0 )
            {
                for (int i = 0; i < 10; ++i)
                {
                    var body = new
                    {
                        Timestamp = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(i))
                    };

                    events.Add(body);
                }
            }

            NameValueCollection keyFields = new NameValueCollection();

            keyFields.Add(Names.EventKeyFieldDeviceId, deviceId);
            keyFields.Add(Names.EventKeyFieldTargetSite, targetSite);

            await IoTHubClient.SendMessageToIoTHubAsync(connectionString, devices, keyFields, events, messageContent);
        }

        private static async Task SendEventsFromCSVFile(string deviceId, string targetSite, string fileDataPath, string fieldDefinitionsPath )
        {
            try
            {
                EventsContainer events = null;

                if(fieldDefinitionsPath.Length > 0 )
                {
                    events = new EventsContainer(fieldDefinitionsPath);
                }
                else
                {
                    events = new EventsContainer(fileDataPath);

                }

                if ( events.EventsFlag )
                {
                    NameValueCollection keyFields = new NameValueCollection();

                    if( deviceId.Length != 0 )
                    {
                        keyFields.Add(Names.EventKeyFieldDeviceId, deviceId);
                        keyFields.Add(Names.EventKeyFieldTargetSite, targetSite);
                    }

                    bool continueProcess = events.ReplayFlag;

                    if (continueProcess)
                        Console.WriteLine("Press Any Key to Halt Replay Process");

                    int iterationsCount = 0;
                    do
                    {
                        List<Task> tasks = new List<Task>(devices.Count());
                        foreach (string[] messageValues in events.GetValuesList())
                        {
                            List<object> dataEvents = new List<object>();

                            dataEvents.Add(events.GetEventMessageForValues(messageValues));

                            if (deviceId.Length == 0)
                            {
                                keyFields = events.GetKeyFields(messageValues);
                            }

                            int waitPeriod = 0;
                            if (int.TryParse(messageValues[0], out waitPeriod))
                            {
                                Thread.Sleep(waitPeriod);
                            }

                            tasks.Add(IoTHubClient.SendMessageToIoTHubAsync(connectionString, devices, keyFields, dataEvents));
                        }
                        
                        await Task.WhenAll(tasks);
                        await Task.Delay(TimeSpan.FromSeconds(1));

                        if (events.ReplayFlag)
                            Console.WriteLine("  Finished Iteration {0}", ++iterationsCount);

                        if (Console.KeyAvailable)
                            continueProcess = false;

                    } while (continueProcess);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send failed. {0}", ex.Message);
            }
        }

        private static async Task SendEventsFromJSONFile(string deviceId, string targetSite, string fileDataPath )
        {
            try
            {
                StreamReader reader = new StreamReader(fileDataPath);
                string messageContent = reader.ReadToEnd();
                reader.Close();
                
                if( messageContent != null && messageContent.Length > 0 )
                    await SendDeviceToCloudMessagesAsync(deviceId, targetSite, messageContent);
                else
                    Console.WriteLine("Message file {0} is empty or unreadable", fileDataPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send failed. {0}", ex.Message);
            }
        }

        private static async Task AddRandomDevicesAsync(int count)
        {
            int start = devices.Count();

            for (int i = start; i < start + count; ++i)
            {
                await AddDeviceAsync("device" + i);
            }
        }

        private static async Task AddDeviceAsync(string deviceId)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            try
            {
                await registryManager.AddDeviceAsync(new Device(deviceId));
                Console.WriteLine("Added device {0}", deviceId);
            }
            catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceAlreadyExistsException)
            {
            }
        }
    }
}
