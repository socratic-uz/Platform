using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smart.Web.Services;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastLevel Level { get; set; } = ToastLevel.Info;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DurationMs { get; set; } = 4000;
}

public class SmartToastService
{
    public event Action<ToastMessage>? OnToastAdded;
    public event Action<string>? OnToastRemoved;

    public void ShowToast(string message, string title = "", ToastLevel level = ToastLevel.Info, int durationMs = 4000)
    {
        var toast = new ToastMessage
        {
            Title = title,
            Message = message,
            Level = level,
            DurationMs = durationMs
        };
        OnToastAdded?.Invoke(toast);
    }

    public void ShowSuccess(string message, string title = "Успех") => ShowToast(message, title, ToastLevel.Success);
    public void ShowError(string message, string title = "Ошибка") => ShowToast(message, title, ToastLevel.Error, 6000);
    public void ShowInfo(string message, string title = "Информация") => ShowToast(message, title, ToastLevel.Info);
    public void ShowWarning(string message, string title = "Внимание") => ShowToast(message, title, ToastLevel.Warning, 5000);

    public void RemoveToast(string id)
    {
        OnToastRemoved?.Invoke(id);
    }
}
