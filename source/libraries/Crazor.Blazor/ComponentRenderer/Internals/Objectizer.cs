// --------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// --------------------------------------------------------------

using AdaptiveCards;
using Crazor.Blazor.Components.Adaptive;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;

namespace Crazor.Blazor.ComponentRenderer.Internals;

// Adapted from BlazorUnitTestingPrototype (Steve Sanderson)
// and https://source.dot.net/#Microsoft.AspNetCore.Mvc.ViewFeatures/RazorComponents/HtmlRenderer.cs
[SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Not change at this moment")]
internal static class Objectizer<ObjectT>
{
    public static ObjectT GetObject(CustomRenderer renderer, int componentId)
    {
        var frames = renderer.GetCurrentRenderTreeFrames(componentId);
        var context = new ObjectRenderingContext(renderer);
        var newPosition = RenderFrames(context, frames, 0, frames.Count);
        Debug.Assert(newPosition == frames.Count);
        return (ObjectT)context.Object;
    }

    private static int RenderChildComponent(
        ObjectRenderingContext context,
        ArrayRange<RenderTreeFrame> frames,
        int position)
    {
        ref var frame = ref frames.Array[position];

        if (frame.Component is IChildItem ci)
        {
            ci.AddToParent();
        }

        if (context.Object == null && frame.Component.GetType() == typeof(ObjectT))
        {
            // capture first card as result
            context.Object = frame.Component;
        }

        var childFrames = context.Renderer.GetCurrentRenderTreeFrames(frame.ComponentId);
        RenderFrames(context, childFrames, 0, childFrames.Count);
        return position + frame.ComponentSubtreeLength;
    }

    private static int RenderChildren(ObjectRenderingContext context, ArrayRange<RenderTreeFrame> frames, int position, int maxElements)
    {
        if (maxElements == 0)
        {
            return position;
        }

        return RenderFrames(context, frames, position, maxElements);
    }

    private static int RenderCore(
        ObjectRenderingContext context,
        ArrayRange<RenderTreeFrame> frames,
        int position)
    {
        ref var frame = ref frames.Array[position];
        switch (frame.FrameType)
        {
            case RenderTreeFrameType.Element:
                return RenderElement(context, frames, position);

            case RenderTreeFrameType.Attribute:
                throw new InvalidOperationException($"Attributes should only be encountered within {nameof(RenderElement)}");

            case RenderTreeFrameType.Text:
                // context.Result.Add(frame.TextContent);
                return ++position;

            case RenderTreeFrameType.Markup:
                // context.Result.Add(frame.MarkupContent);
                return ++position;

            case RenderTreeFrameType.Component:
                return RenderChildComponent(context, frames, position);

            case RenderTreeFrameType.Region:
                return RenderFrames(context, frames, position + 1, frame.RegionSubtreeLength - 1);

            case RenderTreeFrameType.ElementReferenceCapture:
            case RenderTreeFrameType.ComponentReferenceCapture:
                return ++position;

            default:
                throw new InvalidOperationException($"Invalid element frame type '{frame.FrameType}'.");
        }
    }

    private static int RenderElement(
        ObjectRenderingContext context,
        ArrayRange<RenderTreeFrame> frames,
        int position)
    {
        ref var frame = ref frames.Array[position];
        var afterAttributes = RenderAttributes(context, frames, position + 1, frame.ElementSubtreeLength - 1, out var capturedValueAttribute);

        var remainingElements = frame.ElementSubtreeLength + position - afterAttributes;
        if (remainingElements > 0)
        {
            var afterElement = RenderChildren(context, frames, afterAttributes, remainingElements);

            Debug.Assert(afterElement == position + frame.ElementSubtreeLength);
            return afterElement;
        }
        else
        {
            Debug.Assert(afterAttributes == position + frame.ElementSubtreeLength);
            return afterAttributes;
        }
    }


    private static int RenderAttributes(
        ObjectRenderingContext context,
        ArrayRange<RenderTreeFrame> frames,
        int position,
        int maxElements,
        out string? capturedValueAttribute)
    {
        capturedValueAttribute = null;

        if (maxElements == 0)
        {
            return position;
        }

        for (var i = 0; i < maxElements; i++)
        {
            var candidateIndex = position + i;
            ref var frame = ref frames.Array[candidateIndex];
            if (frame.FrameType != RenderTreeFrameType.Attribute)
            {
                return candidateIndex;
            }
        }

        return position + maxElements;
    }

    private static int RenderFrames(ObjectRenderingContext context, ArrayRange<RenderTreeFrame> frames, int position, int maxElements)
    {
        var nextPosition = position;
        var endPosition = position + maxElements;
        while (position < endPosition)
        {
            nextPosition = RenderCore(context, frames, position);
            if (position == nextPosition)
            {
                throw new InvalidOperationException("We didn't consume any input.");
            }
            position = nextPosition;
        }

        return nextPosition;
    }

    private class ObjectRenderingContext
    {
        public ObjectRenderingContext(CustomRenderer renderer)
        {
            Renderer = renderer;
        }

        public string? ClosestSelectValueAsString { get; set; }

        public CustomRenderer Renderer { get; }

        public object Object { get; set; }
    }
}