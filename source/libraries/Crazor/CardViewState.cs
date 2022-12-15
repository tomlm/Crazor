// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using System.Diagnostics;

namespace Crazor
{
    [DebuggerDisplay("{Name}")]
    public class CardViewState 
    {
        public CardViewState() 
        { 
        }

        public CardViewState(Type cardViewType, object? model=null)
        {
            Name = cardViewType.FullName;
            if (model is CardApp)
            {
                throw new ArgumentException("CardApp can't be the model");
            }
            else
            {
                Model = model;
            }
        }

        // name of the card view
        public string Name { get; set; } = String.Empty;

        public bool Initialized { get; set; }

        // card view model
        public object? Model { get; set; } = null;

        // any cardview [sessionMemory] properties are stored here.
        public Dictionary<string, object> SessionMemory { get; set; } = new Dictionary<string, object>();
    }
}
