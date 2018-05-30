// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using global::Iot.Common;

namespace TargetSolution
{
    public static class Names
    {
        public const string EventLatestDictionaryName = "store://events/latest/dictionary";
        public const string EventHistoryDictionaryName = "store://events/history/dictionary";

        public const long DataOffloadBatchIntervalInSeconds = 600;   // this application is not doing any data offload - this timer is only a placehoder;
        public const int DataOffloadBatchSize = 100;
        public const int DataDrainIteration = 5;
    }
}
