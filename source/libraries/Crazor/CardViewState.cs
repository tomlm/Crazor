// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using System.Diagnostics;

namespace Crazor
{
    /// <summary>
    /// CardViewState
    /// </summary>
    /// <remarks>
    /// CardViewState contains the persisted state for a route which is pushed onto the callstack.
    /// The Route is run throuh RouteResolver to instantiate the appropriate CardView 
    /// </remarks>
    [DebuggerDisplay("{Name}")]
    public class CardViewState
    {
        public CardViewState() { }

        public CardViewState(string route, object? model = null)
        {
            Route = CardRoute.Parse(route).Route;
            if (model is CardApp)
            {
                throw new ArgumentException("CardApp can't be the model");
            }
            Model = model;
        }

        /// <summary>
        /// Route this state represents
        /// </summary>
        public string Route { get; set; } = String.Empty;

        public bool Initialized { get; set; }

        // card view model
        public object? Model { get; set; } = null;

        // any cardview [sessionMemory] properties are stored here.
        public Dictionary<string, object> SessionMemory { get; set; } = new Dictionary<string, object>();
    }
}
