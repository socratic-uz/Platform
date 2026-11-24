using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Shared.Layout
{
    public partial class RedirectToSignIn : ComponentBase
    {
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected override Task OnInitializedAsync()
        {
            var isFile = Nav.BaseUri.StartsWith("file:", StringComparison.OrdinalIgnoreCase);
            var rel = Nav.ToBaseRelativePath(Nav.Uri);
            var q = rel.IndexOf('?');
            var path = "/" + (q >= 0 ? rel[..q] : rel).TrimStart('/');
            var returnUrl = q >= 0 ? path + Uri.EscapeDataString(rel[q..]) : path;
            Nav.NavigateTo($"{(isFile ? "signin" : "/signin")}?ReturnUrl={returnUrl}", replace: true);
            return Task.CompletedTask;
        }
    }
}
