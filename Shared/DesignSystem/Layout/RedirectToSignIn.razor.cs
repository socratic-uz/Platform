using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Shared.Layout
{
    public partial class RedirectToSignIn : ComponentBase
    {
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Nav.NavigateTo($"/signin?ReturnUrl={Uri.EscapeDataString(Nav.Uri)}");
            await Task.CompletedTask;
        }
    }
}
