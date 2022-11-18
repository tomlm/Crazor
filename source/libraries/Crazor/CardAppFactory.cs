using Microsoft.AspNetCore.Routing;
using Microsoft.Bot.Schema;
using Neleus.DependencyInjection.Extensions;
using System.Xml.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Crazor
{
    public class CardAppFactory
    {
        private IServiceByNameFactory<CardApp> _cardAppByName;

        public CardAppFactory(IServiceByNameFactory<CardApp> serviceByName)
        {
            _cardAppByName = serviceByName;
        }

        public IEnumerable<string> GetNames() => _cardAppByName.GetNames().OrderBy(n => n);

        public CardApp Create(string name)
        {
            var cardName = _cardAppByName.GetNames().SingleOrDefault(cardName => String.Equals(cardName, name, StringComparison.OrdinalIgnoreCase));
            if (cardName != null)
            {
                var cardApp = _cardAppByName.GetByName(cardName);
                if (cardApp.GetType() == typeof(CardApp))
                {
                    cardApp.Name = cardName;
                }
                return cardApp;
            }
            return null;
        }


        public CardApp CreateFromUri(Uri uri, out string? sharedId, out string view, out string path, out string query)
        {
            CardApp.ParseUri(uri, out var app, out sharedId, out view, out path, out query);

            var cardApp = Create(app);
            return cardApp;
        }
    }
}