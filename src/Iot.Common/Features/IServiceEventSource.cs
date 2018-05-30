using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Fabric;

namespace Iot.Common
{
    public interface IServiceEventSource
    {
        void Message(string message);
        void Message(string message, params object[] args);
        void ServiceMessage(ServiceContext serviceContext, string message, params object[] args);
        void ServiceTypeRegistered(int hostProcessId, string serviceType);
        void ServiceHostInitializationFailed(string exception);
        void ServiceRequestStart(string requestTypeName);
        void ServiceRequestStop(string requestTypeName);
        void ServiceRequestFailed(string requestTypeName, string exception);
    }
}
