// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace QuickGrid.Infrastructure;

// This is used by Table to move its body rendering to the end of the render queue so we can collect
// the list of child columns first. It has to be public only because it's used from .razor logic.

/// <summary>
/// For public use only. Do not use.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Defer : ComponentBase
{
    /// <summary>
    /// For public use only. Do not use.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// For public use only. Do not use.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }
}
