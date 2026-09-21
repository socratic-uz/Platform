using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SharedKernel.ValueObjects;
using SharedKernel.Abstractions;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Interfaces;

namespace Shared.Components
{
    public partial class OrganizationSubscriptionCard : ComponentBase
    {
        [Parameter] public Organization? Organization { get; set; }
        [Parameter] public EventCallback<OrganizationSubscription> OnSubscriptionChanged { get; set; }
        [Inject] public IServiceClientFactory ClientFactory { get; set; } = default!;

        public OrganizationSubscription Subscription => Organization?.Subscription ?? OrganizationSubscription.CreateDefaultTrial();

        public bool isChangePlanModalOpen = false;
        public SubscriptionTier selectedTierForPayment = SubscriptionTier.ProAdFree;
        public string selectedPaymentMethod = "click";

        public string GetTierBadgeClass() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? "badge-trial" : "badge-expired",
            SubscriptionTier.WithAds => "badge-with-ads",
            SubscriptionTier.ProAdFree => "badge-pro",
            _ => "badge-default"
        };

        public string GetTierIcon() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? "hourglass_top" : "warning",
            SubscriptionTier.WithAds => "campaign",
            SubscriptionTier.ProAdFree => "verified",
            _ => "info"
        };

        public string GetTierBadgeTitle() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? "Пробный период (30 дней)" : "Пробный период завершен",
            SubscriptionTier.WithAds => "Тариф «С рекламой»",
            SubscriptionTier.ProAdFree => "Тариф «PRO без рекламы»",
            _ => "Тариф организации"
        };

        public string GetTierHeadline() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive
                ? $"Ваш пробный период активен еще {Subscription.RemainingTrialDays} дней"
                : "Пробный период подошел к концу",
            SubscriptionTier.WithAds => "Подключен тариф «С рекламой» (5$/мес)",
            SubscriptionTier.ProAdFree => "Подключен тариф «PRO без рекламы» (25$/мес)",
            _ => "Управление тарифом"
        };

        public string GetTierDescription() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive
                ? "Вы пользуетесь полным функционалом без сторонней рекламы. По истечении 30 дней заведение автоматически перейдет на тариф с рекламой (5$/мес), если не оформлен PRO."
                : "Чтобы вернуть брендовый экранозаставка без сторонней рекламы, оформите тариф PRO (25$/мес).",
            SubscriptionTier.WithAds => "На экранах ваших киосков транслируются спонсорские баннеры партнеров. Вы можете отключить рекламу в любой момент, перейдя на PRO.",
            SubscriptionTier.ProAdFree => "На всех киосках и кассах отключена сторонняя реклама. Доступен фирменный screensaver с вашим логотипом и фото блюд.",
            _ => ""
        };

        public void OpenSelectPlanDialog(SubscriptionTier tier)
        {
            selectedTierForPayment = tier;
            isChangePlanModalOpen = true;
        }

        public void CloseSelectPlanDialog()
        {
            isChangePlanModalOpen = false;
        }

        public async Task ConfirmPlanChange()
        {
            if (Organization != null)
            {
                Organization.Subscription.Tier = selectedTierForPayment;
                if (selectedTierForPayment == SubscriptionTier.ProAdFree)
                {
                    Organization.Subscription.SubscriptionEndsAt = DateTime.UtcNow.AddMonths(1);
                }
                else if (selectedTierForPayment == SubscriptionTier.WithAds)
                {
                    Organization.Subscription.SubscriptionEndsAt = DateTime.UtcNow.AddMonths(1);
                    Organization.Subscription.TrialEndsAt = DateTime.UtcNow.AddMinutes(-1);
                }

                try
                {
                    var client = ClientFactory.GetClient<Organization>();
                    if (client != null)
                    {
                        await client.Update(Organization);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Subscription] Error updating plan: {ex.Message}");
                }

                if (OnSubscriptionChanged.HasDelegate)
                {
                    await OnSubscriptionChanged.InvokeAsync(Organization.Subscription);
                }
            }

            isChangePlanModalOpen = false;
            StateHasChanged();
        }
    }
}
