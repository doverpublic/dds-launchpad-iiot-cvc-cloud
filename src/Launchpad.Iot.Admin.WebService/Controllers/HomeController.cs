// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        [Route("/healthProbe")]
        public IActionResult HealthProbe()
        {
            ServiceEventSource.Current.Message("Launchpad Admin - Health Probe From Azure");

            return Ok();
        }

        public IActionResult Error()
        {
            return this.View();
        }
    }
}
