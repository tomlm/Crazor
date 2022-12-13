// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Crazor.Blazor.ComponentRenderer.Internals;

namespace Crazor.Blazor.ComponentRenderer;

/// <summary>
/// Templating host that supports service injection and rendering
/// </summary>
public class RenderingContext
{
    private readonly IServiceProvider _serviceProvider;

    public RenderingContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Render a component to HTML
    /// </summary>
    /// <typeparam name="TComponent">the Type of the component</typeparam>
    /// <param name="parameters">Optional dictionary of parameters</param>
    /// <returns></returns>
    public RenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
        where TComponent : IComponent
    {
        var renderer = new HtmlRenderer(_serviceProvider);
        var renderedComponent = new RenderedComponent<TComponent>(renderer, parameters);
        return renderedComponent;
    }

    /// <summary>
    /// Render a component to HTML
    /// </summary>
    /// <typeparam name="TComponent">the Type of the component</typeparam>
    /// <param name="parameters">Optional dictionary of parameters</param>
    /// <returns></returns>
    public RenderedComponent<TComponent> RenderComponent<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>> parameterBuilder)
        where TComponent : IComponent
    {
        var parameters = new ComponentParameterCollectionBuilder<TComponent>(parameterBuilder).Build();
        return RenderComponent<TComponent>(parameters.ToArray());
    }

    /// <summary>
    /// Render a component to HTML
    /// </summary>
    /// <param name="componentType">the Type of the component</param>
    /// <param name="parameters">Optional dictionary of parameters</param>
    /// <returns></returns>
    public RenderedComponent<IComponent> RenderComponent(Type componentType, params ComponentParameter[] parameters)
    {
        var renderer = new HtmlRenderer(_serviceProvider);
        var renderedComponent = new RenderedComponent<IComponent>(renderer, parameters, componentType);
        return renderedComponent;
    }

    /// <summary>
    /// Render a component to HTML
    /// </summary>
    /// <param name="component">the Type of the component</param>
    /// <returns></returns>
    public RenderedComponent<IComponent> RenderComponent(IComponent component)
    {
        var renderer = new HtmlRenderer(_serviceProvider);
        var componentType = component.GetType();
        var parameters = ComponentParameter.FromComponent(component);
        var renderedComponent = new RenderedComponent<IComponent>(renderer, parameters, componentType);
        return renderedComponent;
    }
}