using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SharedKernel.ValueObjects;
using SharedKernel.Abstractions;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Interfaces;

namespace Shared.Components
{
    public enum PromotionModalStep
    {
        SelectPlan = 0,
        PaymentProcessing = 1,
        Success = 2
    }

    public partial class ProductPromotionModal : ComponentBase
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public Product? Product { get; set; }
        [Parameter] public Guid OrganizationId { get; set; }
        [Parameter] public EventCallback<Product> OnProductPromoted { get; set; }
        [Inject] public IServiceClientFactory ClientFactory { get; set; } = default!;

        public PromotionModalStep CurrentStep { get; private set; } = PromotionModalStep.SelectPlan;
        public ProductPromotionPlanDefinition SelectedPlan { get; private set; } = ProductPromotionPlanDefinition.AllPlans[1]; // Monthly by default
        public PromotionPaymentGatewayType SelectedGateway { get; set; } = PromotionPaymentGatewayType.UzQr;
        public ProductPromotionOrder? ActiveOrder { get; private set; }

        public bool isVerifyingPayment = false;

        public void SelectPlan(ProductPromotionPlanDefinition plan)
        {
            SelectedPlan = plan;
        }

        public void ProceedToPayment()
        {
            if (Product == null) return;

            ActiveOrder = new ProductPromotionOrder
            {
                ProductId = Product.Id?.ToString() ?? "",
                ProductName = Product.Name ?? "Товар",
                OrganizationId = OrganizationId.ToString(),
                PlanType = SelectedPlan.PlanType,
                PriceUzs = SelectedPlan.PriceUzs,
                PaymentGateway = SelectedGateway,
                CreatedAt = DateTime.UtcNow,
                QrData = GenerateUzQrPayload()
            };

            CurrentStep = PromotionModalStep.PaymentProcessing;
        }

        private string GenerateUzQrPayload()
        {
            // Формируем стандартизированную строку MUNIS QR-Online / UzQR
            var invoiceId = ActiveOrder?.Id ?? Guid.NewGuid().ToString("N");
            var amountStr = ((long)(ActiveOrder?.PriceUzs ?? 149_000m)).ToString();
            return $"0002010102124044https://qr-online.uz/pay?id={invoiceId}&amount={amountStr}&cur=860";
        }

        public string GetQrCodeImageUrl()
        {
            var payload = Uri.EscapeDataString(ActiveOrder?.QrData ?? GenerateUzQrPayload());
            return $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={payload}";
        }

        public string GetGatewayDisplayName(PromotionPaymentGatewayType gateway) => gateway switch
        {
            PromotionPaymentGatewayType.UzQr => "UzQR (MUNIS QR-Online)",
            PromotionPaymentGatewayType.Click => "Click / Click Up",
            PromotionPaymentGatewayType.Payme => "Payme Business",
            PromotionPaymentGatewayType.Uzum => "Uzum Bank",
            _ => "Онлайн-оплата"
        };

        public string GetPaymentGatewayUrl()
        {
            var invoiceId = ActiveOrder?.Id ?? "inv1";
            var amount = (long)(ActiveOrder?.PriceUzs ?? 149_000m);

            return SelectedGateway switch
            {
                PromotionPaymentGatewayType.Click => $"https://my.click.uz/services/pay?service_id=socratic&merchant_id=31157&amount={amount}&transaction_param={invoiceId}",
                PromotionPaymentGatewayType.Payme => $"https://checkout.paycom.uz/checkout?merchant=socratic&amount={amount * 100}&account%5Border_id%5D={invoiceId}",
                PromotionPaymentGatewayType.Uzum => $"https://www.uzumbank.uz/pay?merchant=socratic&amount={amount}&order={invoiceId}",
                _ => $"https://socratic.uz/pay/{invoiceId}"
            };
        }

        public async Task SimulateConfirmPaymentAsync()
        {
            isVerifyingPayment = true;
            StateHasChanged();

            await Task.Delay(1000); // Симуляция проверки статуса инвойса в шлюзе
            isVerifyingPayment = false;

            if (ActiveOrder != null && Product != null)
            {
                ActiveOrder.IsPaid = true;
                ActiveOrder.PaidAt = DateTime.UtcNow;
                ActiveOrder.PromotedUntil = DateTime.UtcNow.AddDays(SelectedPlan.DurationDays);

                // Добавляем тег "promoted" в метаданные товара
                if (Product.Tags == null) Product.Tags = new List<string>();
                if (!Product.Tags.Contains("promoted"))
                {
                    Product.Tags.Add("promoted");
                }
                if (!Product.Tags.Contains("хит"))
                {
                    Product.Tags.Add("хит");
                }

                // Сохраняем товар через сервис
                try
                {
                    var client = ClientFactory.GetClient<Product>();
                    if (client != null)
                    {
                        await client.Update(Product);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Promotion] Error saving product: {ex.Message}");
                }

                if (OnProductPromoted.HasDelegate)
                {
                    await OnProductPromoted.InvokeAsync(Product);
                }

                CurrentStep = PromotionModalStep.Success;
            }

            StateHasChanged();
        }

        public async Task CloseModal()
        {
            IsOpen = false;
            CurrentStep = PromotionModalStep.SelectPlan;
            if (IsOpenChanged.HasDelegate)
            {
                await IsOpenChanged.InvokeAsync(false);
            }
        }
    }
}
