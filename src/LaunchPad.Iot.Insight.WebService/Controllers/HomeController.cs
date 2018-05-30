// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.WebService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Query;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Linq;
    using System.Net.Http.Headers;

    using Newtonsoft.Json;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    using global::Iot.Common;
    using global::Iot.Common.REST;
    using Launchpad.Iot.PSG.Model;

    public class HomeController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly IApplicationLifetime appLifetime;
        private readonly HttpClient httpClient;

        private readonly StatelessServiceContext context;
        private static NameValueCollection appSettings = ConfigurationManager.AppSettings;

        private static readonly string Username = appSettings["pbiUsername"];
        private static readonly string Password = appSettings["pbiPassword"];
        private static readonly string AuthorityUrl = appSettings["authorityUrl"];
        private static readonly string ResourceUrl = appSettings["resourceUrl"];
        private static readonly string ClientId = appSettings["clientId"];
        private static readonly string ApiUrl = appSettings["apiUrl"];
        private static readonly string GroupId = appSettings["groupId"];

        private static readonly string DevicesDataStream01URL = "https://api.powerbi.com/beta/3d2d2b6f-061a-48b6-b4b3-9312d687e3a1/datasets/ac227ec0-5bfe-4184-85b1-a9643778f1e4/rows?key=zrg4K1om2l4mj97GF6T3p0ze3SlyynHWYRQMdUUSC0BWetzC7bF3RZgPMG4ukznAhGub5aPsDXuQMq540X8hZA%3D%3D";

        public HomeController(StatelessServiceContext context, FabricClient fabricClient, HttpClient httpClient, IApplicationLifetime appLifetime )
        {
            this.context = context;
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.appLifetime = appLifetime;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            // Manage session
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

            // if there is an ongoing session this method will make sure to pass along the session information
            // to the view 
            HTTPHelper.IsSessionExpired(HttpContext, this);

            this.ViewData["TargetSite"] = contextUri.GetServiceNameSite();
            this.ViewData["PageTitle"] = "Home";
            this.ViewData["HeaderTitle"] = "Vibration Device Insights";

            ViewBag.RedirectURL = "";
            ViewBag.Message = "";
            return View("Index");
        }

        [HttpGet]
        [Route("/healthProbe")]
        public IActionResult HealthProbe()
        {
            ServiceEventSource.Current.Message("Insight Webservice - Health Probe From Azure");

            return Ok();
        }

        [HttpGet]
        [Route("run/report/{reportName}")]
        [Route("run/report/{reportName}/parm/{reportParm}")]
        [Route("run/report/{reportName}/byKey/{reportParmStart}")]
        [Route("run/report/{reportName}/byKeyRange/{reportParmStart}/{reportParmEnd}")]
        [Route("run/report/{reportName}/byKeyRange/{reportParmStart}/{reportParmEnd}/{numberOfObservations}")]
        [Route("run/report/{reportName}/byKeyRange/{reportParmStart}/{reportParmEnd}/{numberOfObservations}/{minMagnitudeAllowed}")]
        public async Task<IActionResult> EmbedReport( string reportName, string reportParm = null, string reportParmStart = null, string reportParmEnd = null, int numberOfObservations = (-1), int minMagnitudeAllowed = 1)
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

            ViewBag.RedirectURL = "";

            if (HTTPHelper.IsSessionExpired(HttpContext,this))
            {
                return Ok(contextUri.GetServiceNameSiteHomePath());
            }
            else
            {
                this.ViewData["TargetSite"] = contextUri.GetServiceNameSite();
                this.ViewData["PageTitle"] = "Report";
                this.ViewData["HeaderTitle"] = "Last Posted Events";

                string reportUniqueId = FnvHash.GetUniqueId();

                // Now it is time to refresh the data set
                List<DeviceViewModelList> deviceViewModelList = null;
                int resampleSetsLimit = 0;
                var refreshDataresult = false;
                bool publishReportData = true;

                if (reportName.Equals("PSG-VibrationDeviceReport-02"))
                {
                    refreshDataresult = true;
                    publishReportData = false;
                }
                else if (reportName.Equals("PSG-VibrationDeviceReport-01") && reportParm != null)
                    deviceViewModelList = await DevicesController.GetDevicesDataAsync(reportParm, httpClient, fabricClient, appLifetime);
                else
                {
                    resampleSetsLimit = 1;

                    deviceViewModelList = new List<DeviceViewModelList>();
                    ServiceUriBuilder uriBuilder = new ServiceUriBuilder(Names.InsightDataServiceName);
                    Uri serviceUri = uriBuilder.Build();

                    // service may be partitioned.
                    // this will aggregate device IDs from all partitions
                    ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

                    foreach (Partition partition in partitions)
                    {
                        string pathAndQuery = null;
                        int index = 0;
                        float indexInterval = 1F;
                        bool keepLooping = true;
                        int observationsCount = 0;
                        int batchIndex = 0;
                        int batchSize = 10000;


                        while (keepLooping)
                        {
                            if (reportParmEnd == null)
                            {
                                pathAndQuery = $"/api/devices/history/byKey/{reportParmStart}";
                                keepLooping = false;
                            }
                            else if (numberOfObservations != (-1))
                            {
                                pathAndQuery = $"/api/devices/history/byKeyRange/{reportParmStart}/{reportParmEnd}/{batchIndex}/{batchSize}";

                                if (index == 0)
                                {
                                    string getCountPathAndQuery = $"/api/devices/history/count/interval/{reportParmStart}/{reportParmEnd}";
                                    Uri getCountUrl = new HttpServiceUriBuilder()
                                                .SetServiceName(serviceUri)
                                                .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                                                .SetServicePathAndQuery(getCountPathAndQuery)
                                                .Build();

                                    HttpResponseMessage localResponse = await httpClient.GetAsync(getCountUrl, appLifetime.ApplicationStopping);

                                    if (localResponse.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        string localResult = await localResponse.Content.ReadAsStringAsync();

                                        long count = Int64.Parse(localResult);

                                        indexInterval = count / numberOfObservations;

                                        if (indexInterval < 1)
                                            indexInterval = 1;
                                    }
                                }
                            }
                            else if (reportParmEnd != null)
                            {
                                pathAndQuery = $"/api/devices/history/byKeyRange/{reportParmStart}/{reportParmEnd}";
                                keepLooping = false;
                            }

                            Uri getUrl = new HttpServiceUriBuilder()
                                .SetServiceName(serviceUri)
                                .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                                .SetServicePathAndQuery(pathAndQuery)
                                .Build();

                            HttpResponseMessage response = await httpClient.GetAsync(getUrl, appLifetime.ApplicationStopping);

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                                {
                                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                                    {
                                        List<DeviceViewModelList> localResult = serializer.Deserialize<List<DeviceViewModelList>>(jsonReader);

                                        if (localResult != null)
                                        {
                                            if (localResult.Count != 0)
                                            {
                                                foreach (DeviceViewModelList device in localResult)
                                                {
                                                    if (index >= (observationsCount * indexInterval))
                                                    {
                                                        deviceViewModelList.Add(device);
                                                        observationsCount++;
                                                    }
                                                    index++;

                                                    if (numberOfObservations != (-1))
                                                    {
                                                        if (observationsCount == numberOfObservations)
                                                        {
                                                            keepLooping = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                                keepLooping = false;
                                        }
                                        else
                                        {
                                            keepLooping = false;
                                        }
                                    }
                                }
                            }
                            batchIndex += batchSize;
                        }
                    }
                }

                if (publishReportData)
                    refreshDataresult = await ReportsHandler.PublishReportDataFor(reportUniqueId, DevicesDataStream01URL, deviceViewModelList, context, httpClient, appLifetime.ApplicationStopping, ServiceEventSource.Current, resampleSetsLimit, minMagnitudeAllowed);

                if (reportName.Equals("PSG-VibrationDeviceReport-02"))
                {
                    reportUniqueId = "";
                }

                EmbedConfig task = await ReportsHandler.GetEmbedReportConfigData(ClientId, GroupId, Username, Password, AuthorityUrl, ResourceUrl, ApiUrl, reportUniqueId, reportName, this.context, ServiceEventSource.Current);

                this.ViewData["EmbedToken"] = task.EmbedToken.Token;
                this.ViewData["EmbedURL"] = task.EmbedUrl;
                this.ViewData["EmbedId"] = task.Id;
                this.ViewData["ReportUniqueId"] = reportUniqueId;

                return this.View();
            }
        }

        [HttpGet]
        [Route("run/streamReport/parm/{reportUrl}")]
        public IActionResult EmbedStreamReport(string reportUrl)
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);
            ViewBag.RedirectURL = "";

            if (HTTPHelper.IsSessionExpired(HttpContext, this))
            {
                return Ok(contextUri.GetServiceNameSiteHomePath());
            }
            else
            {
                this.ViewData["EmbedURL"] = reportUrl;
                return this.View();
            }
        }

        [HttpPost]
        [Route("[Controller]/login")]
        public ActionResult Login(UserProfile objUser)
        {
            // Manage session and Context
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

            if (ModelState.IsValid)
            {
                ViewBag.Message = "";
                ViewBag.RedirectURL = "";
                bool newUserRegistration = false;
                bool userAllowedToLogin = false;

                if ((objUser.Password != null && objUser.Password.Length > 0) )
                {
                    // First let deal to see if this a user registration
                    if (objUser.FirstName != null)
                    {
                        newUserRegistration = true;
                        Task<bool> result = RESTHandler.ExecuteFabricPOSTForEntity(typeof(UserProfile),
                                                    Names.InsightDataServiceName,
                                                    "api/entities/user/withIdentity/" + objUser.UserName,
                                                    "user",
                                                    objUser,
                                                    this.context,
                                                    this.httpClient,                                                 
                                                    this.appLifetime.ApplicationStopping,
                                                    ServiceEventSource.Current);
                        if (result.Result)
                            userAllowedToLogin = true;
                        else
                        {
                            ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();
                            ViewBag.Message = "Error during new user registration - User already exist in the database";
                        }
                    }

                    if (!userAllowedToLogin && !newUserRegistration)
                    {
                        Task<object> userObject = RESTHandler.ExecuteFabricGETForEntity(typeof(UserProfile),
                                                    Names.InsightDataServiceName,
                                                    "api/entities/user/byIdentity/" + objUser.UserName,
                                                    "user",
                                                    this.context,
                                                    this.httpClient,
                                                    this.appLifetime.ApplicationStopping,
                                                    ServiceEventSource.Current);
                        if (userObject != null)
                        {
                            UserProfile userProfile = (UserProfile)userObject.Result;

                            if (objUser.Password.Equals(userProfile.Password))
                                userAllowedToLogin = true;
                            else
                            {
                                ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();
                                ViewBag.Message = "Invalid Username and/or Password";
                            }
                        }
                        else
                        {
                            ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();
                            ViewBag.Message = "Error checking user credentials";
                        }
                    }

                    if (userAllowedToLogin)
                    {
                            try
                        {
                            string redirectTo = HTTPHelper.StartSession(HttpContext, this, objUser, "User", "/api/devices", contextUri.GetServiceNameSiteHomePath());

                            //TODO : make the redirection configurable as part of insight application
                            return Redirect(redirectTo);
                        }
                        catch (System.Exception ex)
                        {
                            ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();
                            ViewBag.Message = "Internal Error During User Login- Report to the System Administrator";
                            Console.WriteLine("On Login Session exception msg=[" + ex.Message + "]");
                        }
                    }
                }
                else
                {
                    ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();
                    ViewBag.Message = "Either username and/or password not provided";
                }
            }

            if (!HTTPHelper.IsSessionExpired(HttpContext, this))
                HTTPHelper.EndSession(HttpContext, this);

            return View( "Index", objUser );
        }

        [HttpGet]
        [Route("[Controller]/logout")]
        public IActionResult Logout()
        {
            HttpServiceUriBuilder contextUri = new HttpServiceUriBuilder().SetServiceName(this.context.ServiceName);

            // Manage session
            if (!HTTPHelper.IsSessionExpired(HttpContext, this))
                HTTPHelper.EndSession(HttpContext, this);

            ViewBag.RedirectURL = contextUri.GetServiceNameSiteHomePath();

            return Ok(contextUri.GetServiceNameSiteHomePath());
        }

        public IActionResult About()
        {
            // Manage session
            string sessionId = HTTPHelper.GetCookieValueFor(HttpContext, SessionManager.GetSessionCookieName());

            this.ViewData["Message"] = "Your application description page.";

            return this.View();
        }

        public IActionResult Contact()
        {
            // Manage session
            string sessionId = HTTPHelper.GetCookieValueFor(HttpContext, SessionManager.GetSessionCookieName());

            this.ViewData["Message"] = "Your contact page.";

            return this.View();
        }

        public IActionResult Error()
        {
            return this.View();
        }
    }
}
