using Neleus.DependencyInjection.Extensions;
using System.Xml.Linq;

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
    }
}