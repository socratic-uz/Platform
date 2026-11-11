namespace Shared;

using Microsoft.AspNetCore.Components;

public partial class Routes : ComponentBase
{
    [Inject]
    public NavigationManager Navigation { get; set; } = default!;
}
