// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Fabric;

    public class ServiceUriBuilder
    {
        public ServiceUriBuilder(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public ServiceUriBuilder(string applicationNamePrefix, string siteName, string serviceName)
        {
            this.ApplicationInstance = !applicationNamePrefix.StartsWith("fabric:/")
                ? "fabric:/" + applicationNamePrefix
                : applicationNamePrefix;

            this.ServiceName = serviceName;
            this.SiteName = siteName;
        }

        public ServiceUriBuilder(string applicationNamePrefix, string serviceName)
        {
            this.ApplicationInstance = !applicationNamePrefix.StartsWith("fabric:/")
                ? "fabric:/" + applicationNamePrefix
                : applicationNamePrefix;

            this.ServiceName = serviceName;
        }

        /// <summary>
        /// The name of the application instance that contains he service.
        /// </summary>
        public string ApplicationInstance { get; set; }

        /// <summary>
        /// The name of the service instance.
        /// </summary>
        public string ServiceName { get; set; }

        public string SiteName { get; set; }

        public Uri Build()
        {
            string applicationInstance = this.ApplicationInstance;

            if (String.IsNullOrEmpty(applicationInstance))
            {
                try
                {
                    // the ApplicationName property here automatically prepends "fabric:/" for us
                    applicationInstance = FabricRuntime.GetActivationContext().ApplicationName;
                }
                catch (InvalidOperationException)
                {
                    // FabricRuntime is not available.
                    // This indicates that this is being called from somewhere outside the Service Fabric cluster.
                    applicationInstance = "test";
                }
            }

            string url = applicationInstance.TrimEnd('/') + "/";

            if (this.SiteName != null && this.SiteName.Length > 0)
                url += SiteName.TrimEnd('/');
            else
                url = url.TrimEnd('/');

            return new Uri(url + "/" + this.ServiceName);
        }
    }
}
