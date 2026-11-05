#nullable enable
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Shared.Layout
{
    public partial class SkeletonLoader : ComponentBase
    {
        [Parameter] public int Count { get; set; } = 3;
        [Parameter] public string Type { get; set; } = "card"; // "card", "list", "text"
        [Parameter] public string? Height { get; set; }
        [Parameter] public string? Width { get; set; }
        [Parameter] public string? BorderRadius { get; set; }
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        public string GetCustomStyle() =>
            $"{(!string.IsNullOrEmpty(Height) ? $"height: {Height};" : "")} {(!string.IsNullOrEmpty(Width) ? $"width: {Width};" : "")} {(!string.IsNullOrEmpty(BorderRadius) ? $"border-radius: {BorderRadius};" : "")}";
    }
}
