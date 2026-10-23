using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SharedKernel.ValueObjects;
using SharedKernel.Abstractions;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Interfaces;
using Material.Web;
using static Assets.Constants.Classes.Badge;
using static Assets.Constants.Icons;
using static Assets.Constants.Loc.Subscription;

namespace Shared.Components
{
    public partial class OrganizationSubscriptionCard : ComponentBase
    {
        [Parameter] public Organization? Organization { get; set; }
        [Parameter] public EventCallback<OrganizationSubscription> OnSubscriptionChanged { get; set; }
        [Inject] public IServiceClientFactory ClientFactory { get; set; } = default!;
        [Inject] public IStringLocalizer L { get; set; } = default!;

        public OrganizationSubscription Subscription => Organization?.Subscription ?? OrganizationSubscription.CreateDefaultTrial();

        public bool isChangePlanModalOpen = false;
        public SubscriptionTier selectedTierForPayment = SubscriptionTier.ProAdFree;
        public string selectedPaymentMethod = "click";

        public string GetTierBadgeClass() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? Trial : Expired,
            SubscriptionTier.WithAds => WithAds,
            SubscriptionTier.ProAdFree => Pro,
            _ => "badge-default"
        };

        public string GetTierIcon() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? HourglassTop : Icons.Warning,
            SubscriptionTier.WithAds => Campaign,
            SubscriptionTier.ProAdFree => Verified,
            _ => Info
        };

        public string GetTierBadgeTitle() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive ? L[Org_BadgeTrialActive] : L[Org_BadgeTrialExpired],
            SubscriptionTier.WithAds => L[Org_BadgeWithAds],
            SubscriptionTier.ProAdFree => L[Org_BadgePro],
            _ => L[Org_BadgeDefault]
        };

        public string GetTierHeadline() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive
                ? L[Org_HeadlineTrialActive, Subscription.RemainingTrialDays]
                : L[Org_HeadlineTrialExpired],
            SubscriptionTier.WithAds => L[Org_HeadlineWithAds],
            SubscriptionTier.ProAdFree => L[Org_HeadlinePro],
            _ => L[Org_HeadlineManage]
        };

        public string GetTierDescription() => Subscription.Tier switch
        {
            SubscriptionTier.Trial => Subscription.IsTrialActive
                ? L[Org_DescTrialActive]
                : L[Org_DescTrialExpired],
            SubscriptionTier.WithAds => L[Org_DescWithAds],
            SubscriptionTier.ProAdFree => L[Org_DescPro],
            _ => string.Empty
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
