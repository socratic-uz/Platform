using UiQuality.Analyzer;
using Xunit;

namespace UiQuality.Analyzer.Tests;

public sealed class UiQualityAnalyzerTests
{
    [Fact]
    public void Analyze_reads_component_code_behind_css_and_locations()
    {
        var root = Directory.CreateTempSubdirectory("ui-quality-");
        try
        {
            var razorPath = Path.Combine(root.FullName, "ProductCard.razor");
            File.WriteAllText(razorPath, "@rendermode=\"InteractiveServer\"\n<div class=\"card\">@Title</div>");
            File.WriteAllText(razorPath + ".cs", "using Microsoft.AspNetCore.Components;\npublic partial class ProductCard\n{\n    [Parameter] public required string Title { get; set; }\n}");
            File.WriteAllText(razorPath + ".css", ".card ::deep .title { color: red; }");

            var component = new UiQualityAnalyzer().Analyze(root.FullName).Single();

            Assert.Equal("ProductCard", component.ComponentName);
            Assert.NotNull(component.CodeBehindPath);
            Assert.NotNull(component.IsolatedCssPath);
            Assert.Contains("InteractiveServer", component.RenderModes);
            Assert.Contains("Title", component.Parameters);
            Assert.True(component.UsesDeepSelector);
            Assert.Contains(component.Locations, location => location.Kind == "render-mode" && location.Line == 1);
            Assert.Contains(component.Locations, location => location.Kind == "parameter" && location.FilePath.EndsWith("ProductCard.razor.cs", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public void Analyze_detects_hardcoded_colors_and_touch_target_risks()
    {
        var root = Directory.CreateTempSubdirectory("ui-tokens-");
        try
        {
            var razorPath = Path.Combine(root.FullName, "BadComponent.razor");
            File.WriteAllText(razorPath, "<button style=\"background-color: #1e293b;\">Click</button>");
            File.WriteAllText(razorPath + ".css", ".action-btn { height: 32px; color: #ff0055; background: rgb(255, 255, 255); }");

            var component = new UiQualityAnalyzer().Analyze(root.FullName).Single();

            Assert.Equal("BadComponent", component.ComponentName);
            Assert.Contains("#1e293b", component.HardcodedColors);
            Assert.Contains("#ff0055", component.HardcodedColors);
            Assert.Contains("rgb(255, 255, 255)", component.HardcodedColors);
            Assert.Contains("height: 32px", component.TouchTargetRisks);

            Assert.Contains(component.Locations, l => l.Kind == "hardcoded-inline-color" && l.Value == "#1e293b");
            Assert.Contains(component.Locations, l => l.Kind == "hardcoded-color" && l.Value == "#ff0055");
            Assert.Contains(component.Locations, l => l.Kind == "touch-target-risk" && l.Value == "height: 32px");
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public void Analyze_detects_forbidden_flexbox_usages()
    {
        var root = Directory.CreateTempSubdirectory("ui-flexbox-");
        try
        {
            var razorPath = Path.Combine(root.FullName, "FlexComponent.razor");
            File.WriteAllText(razorPath, "<div style=\"display: inline-flex;\">Inline Flex</div>");
            File.WriteAllText(razorPath + ".css", ".container { display: flex; gap: 8px; }");

            var component = new UiQualityAnalyzer().Analyze(root.FullName).Single();

            Assert.Equal("FlexComponent", component.ComponentName);
            Assert.True(component.UsesFlexbox);
            Assert.Contains("display: flex", component.FlexboxUsages);
            Assert.Contains("display: inline-flex", component.FlexboxUsages);

            Assert.Contains(component.Locations, l => l.Kind == "forbidden-flexbox" && l.Value == "display: flex");
            Assert.Contains(component.Locations, l => l.Kind == "forbidden-inline-flexbox" && l.Value == "display: inline-flex");
        }
        finally
        {
            root.Delete(true);
        }
    }
}

