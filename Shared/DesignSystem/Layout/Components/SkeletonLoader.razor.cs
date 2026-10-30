#nullable enable
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Assets.Constants;
using static Assets.Constants.Classes.Common;
using static Assets.Constants.Classes.Skeleton;
using static Assets.Constants.Classes.Cards;
using static Assets.Constants.Formats;

namespace Shared.Layout
{
    public partial class SkeletonLoader : ComponentBase
    {
        [Parameter] public int Count { get; set; } = 3;
        [Parameter] public string Type { get; set; } = "card"; // "card", "list", "text"
        [Parameter] public string? Height { get; set; }
        [Parameter] public string? Width { get; set; }
        [Parameter] public string? BorderRadius { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        public string GetCustomStyle() =>
            $"{(!string.IsNullOrEmpty(Height) ? $"height: {Height};" : "")} {(!string.IsNullOrEmpty(Width) ? $"width: {Width};" : "")} {(!string.IsNullOrEmpty(BorderRadius) ? $"border-radius: {BorderRadius};" : "")}";

        private string CustomLoaderClass => string.IsNullOrEmpty(Class)
            ? Root
            : string.Format(Two, Root, Class);

        private string ContainerClass => string.IsNullOrEmpty(Class)
            ? string.Format(Three, CardGrid, Root, Type)
            : string.Format(Four, CardGrid, Root, Type, Class);

        private string TextSkeletonClass => string.Format(Two, Root, SkeletonText);

        private string CircleSkeletonClass => string.Format(Two, Root, SkeletonCircle);

        private string CardSkeletonClass => string.Format(Two, M3Card, SkeletonCard);
    }
}
