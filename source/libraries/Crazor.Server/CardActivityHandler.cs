


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder.Teams;
using Newtonsoft.Json;

namespace Crazor.Server
{
    /// <summary>
    /// CardActivityHandler logic.
    /// </summary>
    public partial class CardActivityHandler : TeamsActivityHandler
    {

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        public CardActivityHandler(CardAppContext context)
        {
            Context = context;
        }

        public CardAppContext Context { get; }
    }
}
