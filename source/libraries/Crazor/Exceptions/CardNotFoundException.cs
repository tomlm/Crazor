namespace Crazor.Exceptions
{
    /// <summary>
    /// Throw this exception if the LoadRoute fails to bind.
    /// </summary>
    public class CardRouteNotFoundException : Exception
    {
        public CardRouteNotFoundException(string message) : base(message ?? "Card not found")
        {
        }
    }
}
