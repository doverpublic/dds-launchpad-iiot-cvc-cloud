// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.PSG.Model
{
    using System;
    using System.Collections.Generic;

    public class DeviceViewModelList
    {
        public DeviceViewModelList(string deviceId, IEnumerable<DeviceViewModel> events )
        {
            this.DeviceId = deviceId;
            this.Events = events;
        }

        public string DeviceId { get; private set; }
        public IEnumerable<DeviceViewModel> Events { get; private set; }
    }
}
