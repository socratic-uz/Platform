using System.Text.RegularExpressions;

namespace BlazorUiQuality.Analyzer;

public sealed record BlazorSourceLocation(string FilePath, int Line, int Column, string Kind, string Value);

public sealed record BlazorComponentSource(
    string ComponentName,
    string RazorPath,
    string? CodeBehindPath,
    string? IsolatedCssPath,
    IReadOnlyList<string> RenderModes,
    bool UsesDeepSelector,
    bool UsesFlexbox,
    IReadOnlyList<string> Parameters,
    IReadOnlyList<string> HardcodedColors,
    IReadOnlyList<string> TouchTargetRisks,
    IReadOnlyList<string> FlexboxUsages,
    IReadOnlyList<BlazorSourceLocation> Locations);


public sealed class BlazorSourceAnalyzer
{
    private static readonly Regex RenderModePattern = new(
        "@rendermode(?:\\s*=\\s*|\\s+)[\"']?(?:new\\s+)?([A-Za-z0-9_]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ParameterPattern = new(
        "(?:@parameter\\s+(?:[A-Za-z0-9_<>,.?\\[\\]]+\\s+)?|\\[Parameter(?:Attribute)?(?:\\([^)]*\\))?\\]\\s*(?:(?:public|private|protected|internal)\\s+)?(?:required\\s+)?(?:[A-Za-z0-9_<>,.?\\[\\]]+\\s+)+)([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // Matches hex color codes like #1e293b, #fff, #00ffb3 (avoiding C# preprocessor directives or anchor tags)
    private static readonly Regex HexColorPattern = new(
        @"(?<![\w#])#(?:[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Matches rgb(...) / rgba(...) / hsl(...) / hsla(...) colors
    private static readonly Regex FunctionalColorPattern = new(
        @"\b(?:rgb|rgba|hsl|hsla)\s*\([^)]+\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // Matches small explicit dimensions on touch elements (< 44px)
    private static readonly Regex TouchTargetRiskPattern = new(
        @"(?:min-height|height|min-width|width)\s*:\s*(?:[1-3]?[0-9]|4[0-3])px",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // Matches forbidden flexbox usages (display: flex / display: inline-flex)
    private static readonly Regex FlexboxPattern = new(
        @"display\s*:\s*(?:inline-)?flex\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public IReadOnlyList<BlazorComponentSource> Analyze(string sourceRoot)
    {
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException($"Blazor source root was not found: {sourceRoot}");

        var components = new List<BlazorComponentSource>();
        foreach (var razorPath in Directory.EnumerateFiles(sourceRoot, "*.razor", SearchOption.AllDirectories))
        {
            var content = File.ReadAllText(razorPath);
            var cssPath = razorPath + ".css";
            var codeBehindPath = razorPath + ".cs";
            var cssContent = File.Exists(cssPath) ? File.ReadAllText(cssPath) : null;
            var codeBehindContent = File.Exists(codeBehindPath) ? File.ReadAllText(codeBehindPath) : string.Empty;
            var allSource = content + Environment.NewLine + codeBehindContent;

            var renderModes = RenderModePattern.Matches(allSource).Select(match => match.Groups[1].Value).Distinct().ToArray();
            var parameters = ParameterPattern.Matches(allSource).Select(match => match.Groups[1].Value).Distinct().ToArray();
            var locations = new List<BlazorSourceLocation>();

            AddLocations(locations, razorPath, content, RenderModePattern, "render-mode");
            AddLocations(locations, razorPath, content, ParameterPattern, "parameter");
            if (File.Exists(codeBehindPath))
                AddLocations(locations, codeBehindPath, codeBehindContent, ParameterPattern, "parameter");

            // Check hardcoded colors, touch target risks, and forbidden flexbox in CSS
            var hardcodedColors = new List<string>();
            var touchTargetRisks = new List<string>();
            var flexboxUsages = new List<string>();

            if (!string.IsNullOrEmpty(cssContent))
            {
                AddColorLocations(locations, cssPath, cssContent, hardcodedColors);
                AddTouchTargetLocations(locations, cssPath, cssContent, touchTargetRisks);
                AddFlexboxLocations(locations, cssPath, cssContent, flexboxUsages);
            }

            // Also check inline style="..." in Razor template
            AddInlineStyleColorLocations(locations, razorPath, content, hardcodedColors);
            AddInlineStyleFlexboxLocations(locations, razorPath, content, flexboxUsages);

            var usesFlexbox = flexboxUsages.Count > 0;

            components.Add(new BlazorComponentSource(
                Path.GetFileNameWithoutExtension(razorPath),
                Path.GetFullPath(razorPath),
                File.Exists(codeBehindPath) ? Path.GetFullPath(codeBehindPath) : null,
                File.Exists(cssPath) ? Path.GetFullPath(cssPath) : null,
                renderModes,
                (cssContent?.Contains("::deep", StringComparison.Ordinal) ?? false) || content.Contains("::deep", StringComparison.Ordinal),
                usesFlexbox,
                parameters,
                hardcodedColors.Distinct().ToArray(),
                touchTargetRisks.Distinct().ToArray(),
                flexboxUsages.Distinct().ToArray(),
                locations));
        }

        return components;
    }


    private static void AddLocations(List<BlazorSourceLocation> locations, string filePath, string content, Regex pattern, string kind)
    {
        foreach (Match match in pattern.Matches(content))
        {
            var line = content[..match.Index].Count(character => character == '\n') + 1;
            var lastNewLine = content.LastIndexOf('\n', Math.Max(0, match.Index - 1));
            var column = match.Index - lastNewLine;
            locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, kind, match.Value.Trim()));
        }
    }

    private static void AddColorLocations(List<BlazorSourceLocation> locations, string filePath, string cssContent, List<string> colors)
    {
        foreach (Match match in HexColorPattern.Matches(cssContent))
        {
            var color = match.Value.Trim();
            colors.Add(color);
            var line = cssContent[..match.Index].Count(c => c == '\n') + 1;
            var lastNewLine = cssContent.LastIndexOf('\n', Math.Max(0, match.Index - 1));
            var column = match.Index - lastNewLine;
            locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "hardcoded-color", color));
        }

        foreach (Match match in FunctionalColorPattern.Matches(cssContent))
        {
            var color = match.Value.Trim();
            if (color.Contains("var(--", StringComparison.OrdinalIgnoreCase))
                continue;
            colors.Add(color);
            var line = cssContent[..match.Index].Count(c => c == '\n') + 1;
            var lastNewLine = cssContent.LastIndexOf('\n', Math.Max(0, match.Index - 1));
            var column = match.Index - lastNewLine;
            locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "hardcoded-color", color));
        }
    }

    private static void AddTouchTargetLocations(List<BlazorSourceLocation> locations, string filePath, string cssContent, List<string> risks)
    {
        foreach (Match match in TouchTargetRiskPattern.Matches(cssContent))
        {
            var val = match.Value.Trim();

            // Ignore scrollbar styling or decorative dividers
            var previousBlockStart = cssContent.LastIndexOf('{', match.Index);
            var selector = previousBlockStart > 0
                ? cssContent[Math.Max(0, cssContent.LastIndexOf('}', previousBlockStart) + 1)..previousBlockStart]
                : "";

            if (selector.Contains("scrollbar", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("divider", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("separator", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("border", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("track", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("thumb", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("progress", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("badge", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("score", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("icon", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("svg", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("logo", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("img", StringComparison.OrdinalIgnoreCase) ||
                selector.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Exclude hair-thin dimensions (<= 8px) which are decorative lines, separators, or dots
            var colonIdx = val.IndexOf(':');
            if (colonIdx > 0 && int.TryParse(val[(colonIdx + 1)..].Replace("px", "").Trim(), out var px) && px <= 8)
            {
                continue;
            }

            risks.Add(val);
            var line = cssContent[..match.Index].Count(c => c == '\n') + 1;
            var lastNewLine = cssContent.LastIndexOf('\n', Math.Max(0, match.Index - 1));
            var column = match.Index - lastNewLine;
            locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "touch-target-risk", val));
        }
    }

    private static void AddInlineStyleColorLocations(List<BlazorSourceLocation> locations, string filePath, string razorContent, List<string> colors)
    {
        var styleRegex = new Regex(@"style=[""']([^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        foreach (Match styleMatch in styleRegex.Matches(razorContent))
        {
            var styleBody = styleMatch.Groups[1].Value;
            foreach (Match hexMatch in HexColorPattern.Matches(styleBody))
            {
                var color = hexMatch.Value.Trim();
                colors.Add(color);
                var absoluteIndex = styleMatch.Groups[1].Index + hexMatch.Index;
                var line = razorContent[..absoluteIndex].Count(c => c == '\n') + 1;
                var lastNewLine = razorContent.LastIndexOf('\n', Math.Max(0, absoluteIndex - 1));
                var column = absoluteIndex - lastNewLine;
                locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "hardcoded-inline-color", color));
            }
        }
    }

    private static void AddFlexboxLocations(List<BlazorSourceLocation> locations, string filePath, string cssContent, List<string> flexUsages)
    {
        foreach (Match match in FlexboxPattern.Matches(cssContent))
        {
            var val = match.Value.Trim();
            flexUsages.Add(val);
            var line = cssContent[..match.Index].Count(c => c == '\n') + 1;
            var lastNewLine = cssContent.LastIndexOf('\n', Math.Max(0, match.Index - 1));
            var column = match.Index - lastNewLine;
            locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "forbidden-flexbox", val));
        }
    }

    private static void AddInlineStyleFlexboxLocations(List<BlazorSourceLocation> locations, string filePath, string razorContent, List<string> flexUsages)
    {
        var styleRegex = new Regex(@"style=[""']([^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        foreach (Match styleMatch in styleRegex.Matches(razorContent))
        {
            var styleBody = styleMatch.Groups[1].Value;
            foreach (Match flexMatch in FlexboxPattern.Matches(styleBody))
            {
                var val = flexMatch.Value.Trim();
                flexUsages.Add(val);
                var absoluteIndex = styleMatch.Groups[1].Index + flexMatch.Index;
                var line = razorContent[..absoluteIndex].Count(c => c == '\n') + 1;
                var lastNewLine = razorContent.LastIndexOf('\n', Math.Max(0, absoluteIndex - 1));
                var column = absoluteIndex - lastNewLine;
                locations.Add(new BlazorSourceLocation(Path.GetFullPath(filePath), line, column, "forbidden-inline-flexbox", val));
            }
        }
    }
}

