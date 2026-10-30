using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Ordering.Application.Protos;
using Shopping.Application.Protos;

namespace Shared.Components
{
    public partial class Counter
    {
        protected const Material.Web.Components.ButtonVariant BtnFilled = Material.Web.Components.ButtonVariant.Filled;
        protected const Material.Web.Components.IconButtonVariant IconFilled = Material.Web.Components.IconButtonVariant.Filled;
        protected const Material.Web.Components.IconButtonVariant IconFilledTonal = Material.Web.Components.IconButtonVariant.FilledTonal;
        protected const string RemoveIcon = global::Assets.Constants.Icons.Remove;
        protected const string AddIcon = global::Assets.Constants.Icons.Add;
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public const string CounterQuantityClass = "counter-quantity";

        [Parameter] public EventCallback<(Product?, OrderItem?)> CreateOrderItem { get; set; }
        [Parameter] public EventCallback<OrderItem> Add { get; set; }
        [Parameter] public EventCallback<OrderItem> Remove { get; set; }

        [Parameter] public Product? Product { get; set; } = default!;
        [Parameter] public OrderItem? OrderItem { get; set; } = default!;
    }
}
