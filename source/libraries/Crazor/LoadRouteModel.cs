﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Crazor
{

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LoadRouteModel
    {
        public string? View { get; set; }

        public string? Path { get; set; }
    }
}