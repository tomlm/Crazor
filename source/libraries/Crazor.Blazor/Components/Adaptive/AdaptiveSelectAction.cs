

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
                var property = ParentItem.GetType().GetProperty("SelectAction");
                if (property != null)
                    property.SetValue(ParentItem, value);
                else
                    throw new Exception($"Unknown element {ParentItem?.GetType().Name} as parent for {value.GetType().Name}!");
            }
        }

    }
}
