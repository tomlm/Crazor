namespace Crazor
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
