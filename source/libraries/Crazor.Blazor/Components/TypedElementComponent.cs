// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;


namespace Crazor.Blazor.Components
{
    public class TypedElementComponent<ParentT, TypedElementT> : ItemComponent<ParentT, TypedElementT>
        where TypedElementT : AdaptiveTypedElement
    {
        [Binding(BindingType.PropertyName)]
        [Parameter]
        public string? Id { get => Item.Id; set => Item.Id = value; }
    }
}
