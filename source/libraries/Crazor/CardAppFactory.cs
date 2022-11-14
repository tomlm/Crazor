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
            var cardApp = _cardAppFactory.GetByName(name);
            if (cardApp.GetType() == typeof(CardApp))
            {
                cardApp.Name = name;
            }
            return cardApp;
        }
    }
}