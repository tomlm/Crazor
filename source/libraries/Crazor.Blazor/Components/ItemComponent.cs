// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json;

namespace Crazor.Blazor.Components
{
    public class ItemComponent<ParentT, ItemT> : ComponentBase
    {
        public ItemT Item { get; set; } = Activator.CreateInstance<ItemT>();

        [CascadingParameter]
        public ParentT Parent { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            base.BuildRenderTree(builder);

            // Add the CascadingValue component
            builder.OpenComponent<CascadingValue<ParentT>>(i++);
            builder.AddAttribute(i++, "Name", "Parent");
            builder.AddAttribute(i++, "Value", Item);
            builder.AddAttribute(i++, "ChildContent", (RenderFragment)((builder2) => {
                builder2.AddContent(i++, ChildContent);
            }));
            builder.CloseComponent();

            // Add JSON serialized
            var json = JsonConvert.SerializeObject(Item);
            builder.AddContent(i++, (MarkupString)$"{json},");
        }
    }
}
