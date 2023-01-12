// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Crazor.Server.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("/teams")]
    [ApiController]
    public class ManifestController : ControllerBase
    {
        private string _manifest;

        public ManifestController(Manifest manifest)
        {
            _manifest = JsonConvert.SerializeObject(manifest, Formatting.Indented);
        }

        [HttpGet]
        public IActionResult GetAsync()
        {
            return Content(_manifest, "application/json");
        }
    }
}
