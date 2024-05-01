﻿


namespace Crazor
{
    /// <summary>
    /// CardTableModule which is bound to a single card via path, aka /Cards/MyCard
    /// </summary>
    /// <remarks>This is used by Tab Fetch to process registrations that are entityId path style /Cards/MyCard</remarks>
    internal class SingleCardTabModule : CardTabModule
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SingleCardTabModule(CardAppContext context) : base(context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public void SetRoute(string route)
        {
            if (Uri.TryCreate(route, UriKind.RelativeOrAbsolute, out var uri))
            {
                this.Route = route;
                var cardRoute = CardRoute.Parse(route);
                this.Name = cardRoute.App;
            }
            else
            {
                throw new ArgumentException($"{route} not a uri?");
            }
        }

        public string Route { get; set; }

        public override Task<string[]> GetCardUrisAsync()
        {
            return Task.FromResult(new string[] { Route });
        }
    }
}
