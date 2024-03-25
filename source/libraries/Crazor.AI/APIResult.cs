// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Crazor.AI
{
    public class APIResult
    {
        public APIResult(string name, object result, string message, bool suceeded = true)
        {
            Name = name;
            Suceeded = suceeded;
            Message = message;
            Result = result;
        }

        /// <summary>
        /// Name of API
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("suceeded")]
        public bool Suceeded { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("result")]
        public object Result { get; set; }

        public T GetResult<T>() => ObjectPath.MapValueTo<T>(Result);
    }
}
