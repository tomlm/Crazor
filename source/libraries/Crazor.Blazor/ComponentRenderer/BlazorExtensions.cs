// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------
// Source: https://github.com/bUnit-dev/bUnit
// --------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Text;

namespace Crazor.Blazor.ComponentRenderer;

/// <summary>
/// Extensions for Blazor types.
/// </summary>
public static class BlazorExtensions
{
    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that will render the <paramref name="markup"/>.
    /// </summary>
    /// <param name="markup">Markup to render.</param>
    /// <returns>The <see cref="RenderFragment"/>.</returns>
    public static RenderFragment ToMarkupRenderFragment(this string? markup)
    {
        if (string.IsNullOrEmpty(markup))
            return _ => { };
        return
            builder => builder.AddMarkupContent(0, markup);
    }

    /// <summary>
    /// Gets string content from renderfragment.
    /// </summary>
    /// <param name="renderFragment"></param>
    /// <returns>raw string content</returns>
    public static string GetStringContent(this RenderFragment renderFragment)
    {
#pragma warning disable BL0006 // Do not use RenderTree types
        var builder = new RenderTreeBuilder();
        var stringBuilder = new StringBuilder();
        renderFragment(builder);
        foreach (var item in builder.GetFrames().Array)
        {
            switch (item.FrameType)
            {
                case RenderTreeFrameType.Text:
                    stringBuilder.Append(item.TextContent);
                    break;
                case RenderTreeFrameType.Markup:
                    stringBuilder.Append(item.MarkupContent);
                    break;
                default:
                    break;
            }
        }
#pragma warning restore BL0006 // Do not use RenderTree types
        return stringBuilder.ToString();
    }
}