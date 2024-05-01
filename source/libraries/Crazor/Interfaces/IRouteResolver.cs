


namespace Crazor.Interfaces
{
    public interface IRouteResolver
    {
        bool IsRouteValid(CardRoute route);

        bool ResolveRoute(CardRoute route, out Type? cardViewType);

        bool GetRouteForCardViewType(Type cardViewType, out string route);
    }
}
