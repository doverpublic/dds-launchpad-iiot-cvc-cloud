// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


using System;
using System.Collections.Generic;
using Iot.Common;


namespace Iot.Common.Reports
{
    public class ReportStreamDataset
    {
        public ReportStreamDataset(string reportUniqueId, DatasetConfiguration datasetConfiguration)
        {
            this.ReportUniqueId = reportUniqueId;
            this.DatasetConfiguration = datasetConfiguration;
            this.DeviceMessageList = new List<DeviceMessage>();
        }

        public ReportStreamDataset(string reportUniqueId, DatasetConfiguration datasetConfiguration, List<DeviceMessage> deviceMessagelList)
        {
            this.ReportUniqueId = reportUniqueId;
            this.DatasetConfiguration = datasetConfiguration;
            this.DeviceMessageList = deviceMessagelList;
        }

        public int Count { get { return this.DeviceMessageList.Count; } }
        public DatasetConfiguration DatasetConfiguration { get; private set; }
        public List<DeviceMessage> DeviceMessageList { get; private set; }
        public string ReportUniqueId { get; private set; }

        public void AddMessage( DeviceMessage deviceMessage)
        {
            this.DeviceMessageList.Add(deviceMessage);
        }
    }
}
