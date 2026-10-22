using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Web.UI.Components;

public partial class App : ComponentBase
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    protected string InitialTheme =>
        HttpContext?.Request.Cookies["Theme"]?.ToLowerInvariant() switch
        {
            "light" => "light",
            _ => "dark"
        };
}
