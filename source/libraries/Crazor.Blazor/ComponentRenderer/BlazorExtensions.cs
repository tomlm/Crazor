// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------
// Source: https://github.com/bUnit-dev/bUnit
// --------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Reflection;

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

    public static void AssignInjectAttributeProperties<T>(this T value, IServiceProvider services)
    {
        if (value == null)
        {
            return;
        }

        var props = value.GetType()
                         .GetProperties()
                         .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(InjectAttribute)));

        foreach (var prop in props)
        {            
            prop.SetValue(value, services.GetService(prop.PropertyType), null);
        }
    }
}