// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService.Models
{
    public class InsightApplicationParams
    {
        public InsightApplicationParams(int dataPartitionCount, int webInstanceCount, string version)
        {
            this.DataPartitionCount = dataPartitionCount;
            this.WebInstanceCount = webInstanceCount;
            this.Version = version;
        }

        public int DataPartitionCount { get; set; }

        public int WebInstanceCount { get; set; }

        public string Version { get; set; }
    }
}
