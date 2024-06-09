// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Crazor.Blazor.ComponentRenderer.Internals;

// Adapted from BlazorUnitTestingPrototype (Steve Sanderson)
[SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Not change at this moment")]
internal class ContainerComponent : IComponent
{
    private readonly int _componentId;
    private readonly CustomRenderer _renderer;
    private RenderHandle _renderHandle;

    public ContainerComponent(CustomRenderer renderer)
    {
        _renderer = renderer;
        _componentId = renderer.AttachTestRootComponent(this);
    }

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
    }

    public (int Id, object Instance) FindComponent()
    {
        var ownFrames = _renderer.GetCurrentRenderTreeFrames(_componentId);
        if (ownFrames.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(ContainerComponent)} hasn't yet rendered");
        }

        ref var childComponentFrame = ref ownFrames.Array[0];
        Debug.Assert(childComponentFrame.FrameType == RenderTreeFrameType.Component);
        Debug.Assert(childComponentFrame.Component != null);
        return (childComponentFrame.ComponentId, childComponentFrame.Component);
    }

    public void RenderComponent(Type componentType, ParameterView parameters)
    {
        _renderer.DispatchAndAssertNoSynchronousErrors(() =>
        {
            _renderHandle.Render(builder =>
            {
                builder.OpenComponent(0, componentType);

                foreach (var parameterValue in parameters)
                {
                    builder.AddAttribute(1, parameterValue.Name, parameterValue.Value);
                }

                builder.CloseComponent();
            });
        });
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        throw new NotImplementedException($"{nameof(ContainerComponent)} shouldn't receive any parameters");
    }
}