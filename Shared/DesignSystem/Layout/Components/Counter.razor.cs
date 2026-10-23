using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Ordering.Application.Protos;
using Shopping.Application.Protos;

namespace Shared.Components
{
    public partial class Counter
    {
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public const string CounterQuantityClass = "counter-quantity";

        [Parameter] public EventCallback<(Product?, OrderItem?)> CreateOrderItem { get; set; }
        [Parameter] public EventCallback<OrderItem> Add { get; set; }
        [Parameter] public EventCallback<OrderItem> Remove { get; set; }

        [Parameter] public Product? Product { get; set; } = default!;
        [Parameter] public OrderItem? OrderItem { get; set; } = default!;
    }
}
