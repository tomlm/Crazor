// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Crazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents how a card can be refreshed by making a request to the target Bot
    /// </summary>
    public class AdaptiveSelectAction
    {
        private AdaptiveAction _action;

        public object ParentItem { get; set; }

        public AdaptiveAction Action
        {
            get => _action;
            set
            {
                _action = value;
                ObjectPath.SetPathValue(this.ParentItem, "SelectAction", value, json: false);
            }
        }

    }
}
