// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Text;

namespace CrazorBlazorClientDemo.Client
{
    /// <summary>
    /// RemoteStorage provider, calls /State API on server
    /// </summary>
    public class RemoteStorage : IStorage
    {
        private readonly HttpClient httpClient;

        public RemoteStorage(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            await httpClient.DeleteAsync($"api/state?keys={String.Join(',', keys)}", cancellationToken);
        }

        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            var result = await httpClient.GetStringAsync($"api/state?keys={String.Join(',', keys)}", cancellationToken);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        }

        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            await httpClient.PostAsync($"api/state", new StringContent(JsonConvert.SerializeObject(changes), Encoding.UTF8, "application/json"), cancellationToken);
        }
    }
}
