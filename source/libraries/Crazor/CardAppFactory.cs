using Neleus.DependencyInjection.Extensions;
using System.Xml.Linq;

namespace Crazor
{
    public class CardAppFactory
    {
        private IServiceByNameFactory<CardApp> _cardAppFactory;

        public CardAppFactory(IServiceByNameFactory<CardApp> serviceByName)
        {
            _cardAppFactory = serviceByName;
        }

        public CardApp Create(string name)
        {
            var cardName = _cardAppFactory.GetNames().SingleOrDefault(cardName => String.Equals(cardName, name, StringComparison.OrdinalIgnoreCase));
            if (cardName != null)
            {
                var cardApp = _cardAppFactory.GetByName(cardName);
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