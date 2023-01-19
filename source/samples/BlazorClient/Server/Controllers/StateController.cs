// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrazorBlazorClientDemo.Server.Controllers
{
    [ApiController]
    [Route("api/state")]
    [Produces("application/json")]
    public class StateController : ControllerBase
    {
        private readonly ILogger<StateController> _logger;
        private readonly IStorage _storage;

        public StateController(ILogger<StateController> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        [HttpDelete]
        public async Task DeleteAsync([FromQuery] string[] keys)
        {
            await _storage.DeleteAsync(keys, default);
        }

        [HttpGet]
        public async Task<ContentResult> ReadAsync([FromQuery] string keys)
        {
            var result = await _storage.ReadAsync(keys.Split(','), default);
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        [HttpPost]
        public async Task WriteAsync()
        {
            string body = await new StreamReader(Request.Body).ReadToEndAsync();
            var change  = JObject.Parse(body).ToObject<Dictionary<string, object>>();
            await _storage.WriteAsync(change, default);
        }
    }
}