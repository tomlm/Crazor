// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Crazor.AdaptiveCards;


namespace Crazor.Blazor.Components
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class ActionComponent<ActionT> : TypedElementComponent<ActionT>, IChildItem
        where ActionT : AdaptiveAction
    {
        public virtual void AddToParent()
        {
            if (this.ParentItem is AdaptiveCard card)
            {
                card.Actions.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveActionSet actionSet)
            {
                actionSet.Actions.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveSelectAction selectAction)
            {
                selectAction.Action = this.Item;
            }
            else if (this.ParentItem is AdaptiveTextInput textInput)
            {
                textInput.InlineAction = this.Item;
            }
            else
            {
                throw new Exception("Unknown parent");
            }
        }
    }
}
