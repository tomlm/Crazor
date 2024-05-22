using Crazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents an explict AdaptiveInlineAction
    /// </summary>
    public class AdaptiveInlineAction
    {
        private AdaptiveAction _action;

        public object ParentItem { get; set; }

        public AdaptiveAction Action
        {
            get => _action;
            set
            {
                _action = value;
                var property = ParentItem.GetType().GetProperty("InlineAction");
                if (property != null)
                    property.SetValue(ParentItem, value);
                else
                    throw new Exception($"Unknown element {ParentItem?.GetType().Name} doesn't have InlineAction!");
            }
        }

    }
}
