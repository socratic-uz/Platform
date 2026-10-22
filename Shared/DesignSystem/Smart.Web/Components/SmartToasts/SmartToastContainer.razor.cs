using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Smart.Web.Services;

using Microsoft.Extensions.Localization;

namespace Smart.Web.Components;

public partial class SmartToastContainer : ComponentBase, IDisposable
{
    [Inject] public IStringLocalizer L { get; set; } = default!;
    [Inject] public SmartToastService ToastService { get; set; } = default!;

    protected readonly List<ToastMessage> Toasts = new();

    protected override void OnInitialized()
    {
        ToastService.OnToastAdded += HandleToastAdded;
        ToastService.OnToastRemoved += HandleToastRemoved;
    }

    private void HandleToastAdded(ToastMessage toast)
    {
        InvokeAsync(async () =>
        {
            Toasts.Add(toast);
            StateHasChanged();

            if (toast.DurationMs > 0)
            {
                await Task.Delay(toast.DurationMs);
                DismissToast(toast.Id);
            }
        });
    }

    private void HandleToastRemoved(string id)
    {
        DismissToast(id);
    }

    protected void DismissToast(string id)
    {
        InvokeAsync(() =>
        {
            Toasts.RemoveAll(t => t.Id == id);
            StateHasChanged();
        });
    }

    public void Dispose()
    {
        ToastService.OnToastAdded -= HandleToastAdded;
        ToastService.OnToastRemoved -= HandleToastRemoved;
    }
}
