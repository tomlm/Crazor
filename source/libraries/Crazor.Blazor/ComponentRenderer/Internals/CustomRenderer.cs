// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------

using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.ExceptionServices;
using System.Diagnostics.CodeAnalysis;

namespace Crazor.Blazor.ComponentRenderer.Internals;

// Adapted from BlazorUnitTestingPrototype (Steve Sanderson)
[SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Not change at this moment")]
internal class CustomRenderer : Renderer
{
    private TaskCompletionSource<object?> _nextRenderTcs = new();
    private Exception? _unhandledException;

    public CustomRenderer(IServiceProvider serviceProvider)
        : base(serviceProvider, (ILoggerFactory?)serviceProvider?.GetService(typeof(ILoggerFactory)) ?? new NullLoggerFactory())
    {
    }

    public CustomRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        : base(serviceProvider, loggerFactory)
    {
    }

    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

    public Task NextRender => _nextRenderTcs.Task;

    public int AttachTestRootComponent(ContainerComponent testRootComponent)
    {
        return AssignRootComponentId(testRootComponent);
    }

    public void DispatchAndAssertNoSynchronousErrors(Action callback)
    {
        Dispatcher.InvokeAsync(callback).Wait();
        AssertNoSynchronousErrors();
    }

    public new Task DispatchEventAsync(ulong eventHandlerId, EventFieldInfo fieldInfo, EventArgs eventArgs)
    {
        var task = Dispatcher.InvokeAsync(
            () => base.DispatchEventAsync(eventHandlerId, fieldInfo, eventArgs));
        AssertNoSynchronousErrors();
        return task;
    }

    public new ArrayRange<RenderTreeFrame> GetCurrentRenderTreeFrames(int componentId)
    {
        return base.GetCurrentRenderTreeFrames(componentId);
    }
    protected override void HandleException(Exception exception)
    {
        _unhandledException = exception;
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        var prevTcs = _nextRenderTcs;
        _nextRenderTcs = new();
        prevTcs.SetResult(null);
        return Task.CompletedTask;
    }
    private void AssertNoSynchronousErrors()
    {
        if (_unhandledException != null)
        {
            ExceptionDispatchInfo.Capture(_unhandledException).Throw();
        }
    }
}