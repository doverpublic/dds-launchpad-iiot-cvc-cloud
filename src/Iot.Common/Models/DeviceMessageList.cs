// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Collections.Generic;

    public class DeviceMessageList
    {
        private List<DeviceMessage> rowList = new List<DeviceMessage>();

        public DeviceMessageList(int batchIndex, int batchSize)
        {
            this.BatchIndex = batchIndex;
            this.BatchSize = batchSize;
            this.TotalCount = 0;
            this.SearchStartTimestamp = DateTimeOffset.Parse("1970-01-01T00:00:00.000Z");
            this.Rows = rowList;
        }

        public int BatchIndex { get; private set; }
        public int BatchSize { get; set; }
        public DateTimeOffset SearchStartTimestamp { get; set; }
        public IEnumerable<DeviceMessage> Rows { get; private set; }
        public int TotalCount { get; set; }

        public void AddRow(DeviceMessage row )
        {
            this.rowList.Add(row);
        }
    }
}
