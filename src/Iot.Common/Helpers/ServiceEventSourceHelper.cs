using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Fabric;

namespace Iot.Common
{
    public class ServiceEventSourceHelper : IServiceEventSource
    {
        private IServiceEventSource ServiceEntitySource;

        public ServiceEventSourceHelper(IServiceEventSource serviceEventSource)
        {
            this.ServiceEntitySource = serviceEventSource;
        }

        public void Message(string message)
        {
            if (this.ServiceEntitySource != null)
                this.ServiceEntitySource.Message(message);
        }

        public void Message(string message, params object[] args)
        {
            if (this.ServiceEntitySource != null)
                this.ServiceEntitySource.Message(message,args);
        }

        public void ServiceMessage(ServiceContext serviceContext, string message, params object[] args)
        {
            if (this.ServiceEntitySource != null)
                this.ServiceEntitySource.ServiceMessage(serviceContext, message, args);
        }

        public void ServiceTypeRegistered(int hostProcessId, string serviceType)
        {
            if (this.ServiceEntitySource != null)
            {
                this.ServiceEntitySource.ServiceTypeRegistered(hostProcessId, serviceType);
                Message($"ServiceEventSource.ServiceTypeRegistered called for HostProcessId[{hostProcessId}] ServiceType[{serviceType}]");
            }
        }

        public void ServiceHostInitializationFailed(string exception)
        {
            if (this.ServiceEntitySource != null)
            {
                this.ServiceEntitySource.ServiceHostInitializationFailed(exception);
                Message($"ServiceEventSource.ServiceHostInitializationFailed called for Exception[{exception}]");
            }
        }

        public void ServiceRequestStart(string requestTypeName)
        {
            if (this.ServiceEntitySource != null)
            {
                this.ServiceEntitySource.ServiceRequestStart(requestTypeName);
                Message($"ServiceEventSource.ServiceRequestStart called for Request Type Name[{requestTypeName}]");
            }
        }

        public void ServiceRequestStop(string requestTypeName)
        {
            if (this.ServiceEntitySource != null)
            {
                this.ServiceEntitySource.ServiceRequestStop(requestTypeName);
                Message($"ServiceEventSource.ServiceRequestStop called for Request Type Name[{requestTypeName}]");
            }
        }

        public void ServiceRequestFailed(string requestTypeName, string exception)
        {
            if (this.ServiceEntitySource != null)
            {
                this.ServiceEntitySource.ServiceRequestFailed(requestTypeName, exception);
                Message($"ServiceEventSource.ServiceRequestFailed called for Request Type Name[{requestTypeName}] Exception[{exception}]");
            }
        }
    }
}
