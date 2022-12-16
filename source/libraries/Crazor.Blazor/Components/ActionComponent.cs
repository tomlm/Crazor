// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;


namespace Crazor.Blazor.Components
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class ActionComponent<ActionT> : TypedElementComponent<ActionT>
        where ActionT : AdaptiveAction
    {
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.Parent is AdaptiveCard card)
            {
                card.Actions.Add(this.Item);
            }
            else if (this.Parent is AdaptiveActionSet actionSet)
            {
                actionSet.Actions.Add(this.Item);
            }
            else if (this.Parent is AdaptiveSelectAction selectAction)
            {
                selectAction.Action = this.Item;
            }
            else
            {
                throw new Exception("Unknown parent");
            }
        }
    }
}
