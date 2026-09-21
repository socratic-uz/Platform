using Microsoft.AspNetCore.Components;

using Ordering.Application.Protos;

using Shopping.Application.Protos;

namespace Shared.Components
{
    public partial class Counter
    {
        [Parameter] public EventCallback<(Product?, OrderItem?)> CreateOrderItem { get; set; }
        [Parameter] public EventCallback<OrderItem> Add { get; set; }
        [Parameter] public EventCallback<OrderItem> Remove { get; set; }

        [Parameter] public Product? Product { get; set; } = default!;
        [Parameter] public OrderItem? OrderItem { get; set; } = default!;
    }
}
