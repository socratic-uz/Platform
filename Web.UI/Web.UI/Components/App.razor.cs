using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;

namespace Web.UI.Components;

public partial class App : ComponentBase
{
    private static readonly string[] StaticSsrPrefixes = ["/about", "/error"];

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    protected string InitialTheme =>
        HttpContext?.Request.Cookies["Theme"]?.ToLowerInvariant() switch
        {
            "light" => "light",
            _ => "dark"
        };

    protected bool IsStaticSsr
    {
        get
        {
            var path = HttpContext?.Request.Path.Value;
            if (string.IsNullOrEmpty(path)) return false;
            if (path == "/") return true;

            foreach (var prefix in StaticSsrPrefixes)
            {
                if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    protected IComponentRenderMode? PageRenderMode => IsStaticSsr ? null : RenderMode.InteractiveAuto;
}
