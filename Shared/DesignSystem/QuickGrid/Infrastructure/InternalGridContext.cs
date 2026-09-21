// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace QuickGrid.Infrastructure;

// The grid cascades this so that descendant columns can talk back to it. It's an public type
// so that it doesn't show up by mistake in unrelated components.
public sealed class InternalGridContext<TGridItem>
{
    public QuickGrid<TGridItem> Grid { get; }
    public EventCallbackSubscribable<object?> ColumnsFirstCollected { get; } = new();

    public InternalGridContext(QuickGrid<TGridItem> grid)
    {
        Grid = grid;
    }
}
