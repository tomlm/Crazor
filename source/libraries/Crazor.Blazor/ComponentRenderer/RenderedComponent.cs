// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------

namespace Crazor.Blazor.ComponentRenderer;

using Microsoft.AspNetCore.Components;
using Crazor.Blazor.ComponentRenderer.Internals;
using AdaptiveCards;

// Adapted from BlazorUnitTestingPrototype (Steve Sanderson)
public class RenderedComponent<TComponent> where TComponent : IComponent
{
    private readonly CustomRenderer _renderer;
    private ContainerComponent _containerTestRootComponent { get; }
    private int _componentId;
    private Type _componentType;

    internal RenderedComponent(CustomRenderer renderer, IEnumerable<ComponentParameter>? parameters, Type? componentType = null)
    {
        _renderer = renderer;
        _componentType = componentType ?? typeof(TComponent);

        if (!typeof(IComponent).IsAssignableFrom(_componentType))
            throw new ArgumentException("Type must implement IComponent", nameof(_componentType));

        _containerTestRootComponent = new ContainerComponent(_renderer);

        var item = this.SetParametersAndRender(GetParameterView(parameters));
        _componentId = item.Id;
        // this.Instance = item.Instance;
    }

    public virtual AdaptiveCard Card => Cardizer.GetCard(_renderer, _componentId);

    public virtual string Markup => Htmlizer.GetHtml(_renderer, _componentId);

    internal virtual (int Id, TComponent Instance) SetParametersAndRender(ParameterView parameters)
    {
        _containerTestRootComponent.RenderComponent(_componentType, parameters);
        var foundComponent = _containerTestRootComponent.FindComponent();
        return (foundComponent.Id, (TComponent)foundComponent.Instance);
    }

    private ParameterView GetParameterView(IEnumerable<ComponentParameter>? parameters)
    {
        if (parameters == null)
        {
            return ParameterView.Empty;
        }

        var items = parameters.ToDictionary(i => i.Name ?? string.Empty, i => i.Value);
        return ParameterView.FromDictionary(items);
    }
}