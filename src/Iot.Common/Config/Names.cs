// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    public static class Names
    {
        public const string EVENT_KEY_DEVICE_ID = "deviceId";
        public const string EVENT_KEY_TARGET_SITE = "targetSite";

        public const string REPORTS_SECRET_KEY_NAME = "___ReportsSecretKey___";
        public const string REPORTS_SECRET_KEY_VALUE = "A58528892D4827FCBD37EE9E1CC88DE82EFA08EC";

        public const string EventKeyFieldDeviceId = Iot.Common.Names.EVENT_KEY_DEVICE_ID;
        public const string EventKeyFieldTargetSite = Iot.Common.Names.EVENT_KEY_TARGET_SITE;

        public const string EventsProcessorApplicationPrefix = "fabric:/Launchpad.Iot.EventsProcessor";
        public const string EventsProcessorApplicationTypeName = "LaunchpadIotEventsProcessorApplicationType";
        public const string EventsProcessorExtenderServiceName = "ExtenderService";
        public const string EventsProcessorExtenderServiceTypeName = "ExtenderServiceType";
        public const string EventsProcessorRouterServiceName = "RouterService";
        public const string EventsProcessorRouterServiceTypeName = "RouterServiceType";

        public const string InsightApplicationNamePrefix = "fabric:/Launchpad.Iot.Insight";
        public const string InsightApplicationTypeName = "LaunchpadIotInsightApplicationType";
        public const string InsightDataServiceName = "DataService";
        public const string InsightDataServiceTypeName = "DataServiceType";
        public const string InsightWebServiceName = "WebService";
        public const string InsightWebServiceTypeName = "WebServiceType";

        public const string EntitiesDictionaryName = "store://entities/dictionary";
        public const string IdentitiesDictionaryName = "store://identities/dictionary";

        public const int TransactionsRetryCount = 10;
        public const int TransactionRetryWaitIntervalInMills = 200;

        public const int DataServiceCacheSizeForHistoryObjects = 20;

        public const int IoTHubRetryWaitIntervalsInMills = 60000;
        public const int EventsProcessorOffsetInterval = 5;

        public const int IntervalBetweenReportStreamingCalls = 1200;

        public const int ExtenderStandardRetryWaitIntervalsInMills = 180000;
    }
}
