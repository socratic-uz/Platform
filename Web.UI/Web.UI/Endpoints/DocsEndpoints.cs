using System.Text;

namespace Web.UI.Endpoints;

public static class DocsEndpoints
{
    public static void MapDocsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs", (IWebHostEnvironment env) =>
        {
            var docsRoot = GetDocsRoot(env);
            var files = GetMarkdownFilesResilient(docsRoot)
                .Select(f => Path.GetRelativePath(docsRoot, f).Replace('\\', '/'))
                .OrderBy(f => f)
                .ToList();

            return Results.Ok(files);
        }).DisableAntiforgery();

        app.MapGet("/api/docs/content", (string path, IWebHostEnvironment env) =>
        {
            var docsRoot = GetDocsRoot(env);
            var fullPath = Path.GetFullPath(Path.Combine(docsRoot, path));
            if (!fullPath.StartsWith(docsRoot, StringComparison.OrdinalIgnoreCase) || !fullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Invalid path");
            }

            if (!File.Exists(fullPath))
            {
                return Results.NotFound();
            }

            var content = File.ReadAllText(fullPath);
            return Results.Content(content, "text/markdown", Encoding.UTF8);
        }).DisableAntiforgery();
    }

    public static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (dir.GetFiles("Socratic.slnx").Any() || dir.GetDirectories(".git").Any())
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    public static string GetDocsRoot(IWebHostEnvironment env)
    {
        var solutionRoot = GetSolutionRoot();
        var docsInSolution = Path.Combine(solutionRoot, "docs");
        if (Directory.Exists(docsInSolution) && File.Exists(Path.Combine(solutionRoot, "README.md")))
        {
            return solutionRoot;
        }

        var wwwrootDocs = Path.Combine(env.WebRootPath, "_docs");
        if (Directory.Exists(wwwrootDocs))
        {
            return wwwrootDocs;
        }

        return env.ContentRootPath;
    }

    private static List<string> GetMarkdownFilesResilient(string rootPath)
    {
        var result = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(rootPath);

        while (queue.Count > 0)
        {
            var currentDir = queue.Dequeue();
            try
            {
                var files = Directory.GetFiles(currentDir, "*.md");
                result.AddRange(files);

                var subDirs = Directory.GetDirectories(currentDir);
                foreach (var subDir in subDirs)
                {
                    var dirName = Path.GetFileName(subDir);
                    if (dirName.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("node_modules", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals(".gemini", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals(".agents", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("appdata", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("AppHost", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("CameraStreamer", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Chat", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Map", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Markdown", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Material.Web", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("QuickGrid", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("ServiceDefaults", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Smart.Web", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    queue.Enqueue(subDir);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        return result;
    }
}
