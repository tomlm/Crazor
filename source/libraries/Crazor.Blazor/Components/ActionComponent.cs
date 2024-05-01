


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;


namespace Crazor.Blazor.Components
{
    /// <summary>
    /// ActionComponent is base for adaptive actions
    /// </summary>
    public class ActionComponent<ActionT> : TypedElementComponent<ActionT>, IChildItem
        where ActionT : AdaptiveAction
    {
        // this implements implicit mappings of actions to SelectAction or actions collections.
        public virtual void AddToParent()
        {
            if (this.ParentItem is AdaptiveSelectAction selectAction)
            {
                selectAction.Action = this.Item;
            }
            else if (this.ParentItem is AdaptiveInlineAction inlineAction)
            {
                inlineAction.Action = this.Item;
            }
            else if (this.ParentItem is AdaptiveCard card)
            {
                card.Actions.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveActionSet actionSet)
            {
                actionSet.Actions.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveTextInput textInput)
            {
                textInput.InlineAction = this.Item;
            }
            else if (this.ParentItem is AdaptiveCollectionElement coll)
            {
                coll.SelectAction = this.Item;
            }
            else if (this.ParentItem is AdaptiveImage image)
            {
                image.SelectAction = this.Item;
            }
            else if (this.ParentItem is AdaptiveTextRun textRun)
            {
                textRun.SelectAction = this.Item;
            }
            else
            {
                throw new Exception($"Unknown element {ParentItem?.GetType().Name} as parent for {this.Item.GetType().Name}!");
            }
        }
    }
}
