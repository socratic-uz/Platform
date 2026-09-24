using System.Text.Json;
using BlazorUiQuality.Analyzer;

if (args.Length < 2 || args[0] != "--source-root")
{
    Console.Error.WriteLine("Usage: BlazorUiQuality.Analyzer --source-root <path> [--format json|human]");
    return 2;
}

var sourcePath = Path.GetFullPath(args[1]);
var format = "json";
for (var i = 2; i < args.Length; i++)
{
    if (args[i] == "--format" && i + 1 < args.Length)
    {
        format = args[i + 1].ToLowerInvariant();
    }
    else if (args[i] == "--summary")
    {
        format = "human";
    }
}

try
{
    var components = new BlazorSourceAnalyzer().Analyze(sourcePath);

    if (format == "human")
    {
        Console.WriteLine($"=== Blazor UI Quality Audit Report ({components.Count} components) ===");
        Console.WriteLine($"Source root: {sourcePath}\n");

        var totalHardcoded = components.Sum(c => c.HardcodedColors.Count);
        var totalDeep = components.Count(c => c.UsesDeepSelector);
        var totalTouchRisks = components.Sum(c => c.TouchTargetRisks.Count);
        var totalFlex = components.Sum(c => c.FlexboxUsages.Count);

        Console.WriteLine($"Summary:");
        Console.WriteLine($"  Total Components:     {components.Count}");
        Console.WriteLine($"  ::deep Usages:        {totalDeep}");
        Console.WriteLine($"  Hardcoded Colors:     {totalHardcoded}");
        Console.WriteLine($"  Touch Target Risks:   {totalTouchRisks}");
        Console.WriteLine($"  Forbidden Flexbox:    {totalFlex}\n");

        var flagged = components.Where(c => c.HardcodedColors.Count > 0 || c.TouchTargetRisks.Count > 0 || c.UsesDeepSelector || c.UsesFlexbox).ToList();
        if (flagged.Count > 0)
        {
            Console.WriteLine("Flagged Components:");
            foreach (var c in flagged)
            {
                Console.WriteLine($"  - {c.ComponentName} ({Path.GetFileName(c.RazorPath)})");
                if (c.UsesDeepSelector)
                    Console.WriteLine("      [WARN] Uses ::deep selector");
                foreach (var flex in c.FlexboxUsages)
                    Console.WriteLine($"      [LAYOUT] Forbidden flexbox '{flex}' - use CSS Grid ('display: grid')");
                foreach (var color in c.HardcodedColors)
                    Console.WriteLine($"      [TOKEN] Hardcoded color '{color}' - replace with var(--md-sys-color-*)");
                foreach (var risk in c.TouchTargetRisks)
                    Console.WriteLine($"      [TOUCH] Potential small touch target '{risk}' - ensure >=48x48px");
            }
        }
        else
        {
            Console.WriteLine("All components comply with Material 3 design token and CSS Grid layout rules!");
        }
    }

    else
    {
        Console.WriteLine(JsonSerializer.Serialize(components, AnalyzerJsonContext.Default.IReadOnlyListBlazorComponentSource));
    }
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

[System.Text.Json.Serialization.JsonSerializable(typeof(IReadOnlyList<BlazorComponentSource>))]
[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false)]
internal sealed partial class AnalyzerJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
