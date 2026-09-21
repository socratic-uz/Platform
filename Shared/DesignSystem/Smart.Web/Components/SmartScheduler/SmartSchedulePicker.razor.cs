using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SharedKernel.ValueObjects;

namespace Smart.Web.Components;

public partial class SmartSchedulePicker : ComponentBase
{
    [Inject] private IStringLocalizer? L { get; set; }

    [Parameter] public WorkSchedule Schedule { get; set; } = new();
    [Parameter] public EventCallback<WorkSchedule> ScheduleChanged { get; set; }
    [Parameter] public EventCallback OnReset { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;

    private string EffectiveLabel => Label ?? (L?["Schedule_Title"] ?? "График работы");

    private bool _isDialogOpen = false;

    private bool HasActiveDays => Schedule?.WeeklyItems != null && Schedule.WeeklyItems.Any(w => !w.IsClosed && w.Intervals != null && w.Intervals.Count > 0);
    private int ActiveDaysCount => Schedule?.WeeklyItems?.Count(w => !w.IsClosed && w.Intervals != null && w.Intervals.Count > 0) ?? 0;

    private void OpenDialog()
    {
        if (Disabled) return;
        _isDialogOpen = true;
        StateHasChanged();
    }

    private void CloseDialog()
    {
        _isDialogOpen = false;
        StateHasChanged();
    }

    private async Task SaveAndClose()
    {
        _isDialogOpen = false;
        await NotifyChanged();
    }

    private string GetSummaryText()
    {
        if (Schedule == null || Schedule.WeeklyItems == null || Schedule.WeeklyItems.Count == 0)
            return L?["Schedule_NotSet"] ?? "График работы не задан";

        var activeItems = Schedule.WeeklyItems.Where(w => !w.IsClosed && w.Intervals != null && w.Intervals.Count > 0).ToList();
        if (!activeItems.Any()) return L?["Schedule_AllWeekends"] ?? "Выходные дни на всю неделю";

        var first = activeItems.First().Intervals.First();
        return $"{L?["Schedule_Active"] ?? "Активен"}: {activeItems.Count} {L?["Schedule_DaysShort"] ?? "дн."} ({FormatTime(first.Start)} — {FormatTime(first.End)})";
    }

    private bool IsDayActive(DayOfWeek day)
    {
        if (Schedule?.WeeklyItems == null) return false;
        var item = Schedule.WeeklyItems.FirstOrDefault(w => w.DayOfWeek == day);
        return item != null && !item.IsClosed && item.Intervals != null && item.Intervals.Count > 0;
    }

    private WeeklyScheduleItem GetOrCreateItem(DayOfWeek day)
    {
        Schedule.WeeklyItems ??= new();
        var item = Schedule.WeeklyItems.FirstOrDefault(w => w.DayOfWeek == day);
        if (item == null)
        {
            item = new WeeklyScheduleItem
            {
                DayOfWeek = day,
                IsClosed = true,
                Intervals = new List<TimeInterval>()
            };
            Schedule.WeeklyItems.Add(item);
        }
        return item;
    }

    private async Task ToggleDay(DayOfWeek day, bool active)
    {
        var item = GetOrCreateItem(day);
        item.IsClosed = !active;
        if (active)
        {
            item.Intervals ??= new List<TimeInterval>();
            if (item.Intervals.Count == 0)
            {
                item.Intervals.Add(new TimeInterval(new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0)));
            }
        }
        else
        {
            item.Intervals?.Clear();
        }
        await NotifyChanged();
    }

    private async Task UpdateOpenTime(DayOfWeek day, string? timeStr)
    {
        var item = GetOrCreateItem(day);
        if (TimeSpan.TryParse(timeStr, out var ts))
        {
            item.Intervals ??= new List<TimeInterval>();
            var end = item.Intervals.FirstOrDefault()?.End ?? new TimeSpan(18, 0, 0);
            item.Intervals.Clear();
            item.Intervals.Add(new TimeInterval(ts, end));
            item.IsClosed = false;
        }
        await NotifyChanged();
    }

    private async Task UpdateCloseTime(DayOfWeek day, string? timeStr)
    {
        var item = GetOrCreateItem(day);
        if (TimeSpan.TryParse(timeStr, out var ts))
        {
            item.Intervals ??= new List<TimeInterval>();
            var start = item.Intervals.FirstOrDefault()?.Start ?? new TimeSpan(9, 0, 0);
            item.Intervals.Clear();
            item.Intervals.Add(new TimeInterval(start, ts));
            item.IsClosed = false;
        }
        await NotifyChanged();
    }

    private async Task ApplyWeekdaysPreset()
    {
        var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        foreach (var day in weekdays)
        {
            var item = GetOrCreateItem(day);
            item.IsClosed = false;
            item.Intervals = new List<TimeInterval> { new(new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0)) };
        }

        foreach (var day in weekends)
        {
            var item = GetOrCreateItem(day);
            item.IsClosed = true;
            item.Intervals = new List<TimeInterval>();
        }

        await NotifyChanged();
    }

    private async Task ApplyRetailPreset()
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var item = GetOrCreateItem(day);
            item.IsClosed = false;
            item.Intervals = new List<TimeInterval> { new(new TimeSpan(10, 0, 0), new TimeSpan(22, 0, 0)) };
        }

        await NotifyChanged();
    }

    private async Task Apply247Preset()
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var item = GetOrCreateItem(day);
            item.IsClosed = false;
            item.Intervals = new List<TimeInterval> { new(new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)) };
        }

        await NotifyChanged();
    }

    public async Task ClearSchedule()
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var item = GetOrCreateItem(day);
            item.IsClosed = true;
            item.Intervals = new List<TimeInterval>();
        }

        await NotifyChanged();
        await OnReset.InvokeAsync();
    }

    private async Task OnTimeZoneSelected(string? tz)
    {
        Schedule.TimeZoneId = tz ?? "UTC";
        await NotifyChanged();
    }

    private async Task NotifyChanged()
    {
        await ScheduleChanged.InvokeAsync(Schedule);
        StateHasChanged();
    }

    private string GetDayShort(DayOfWeek day) =>
        CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(day);

    private string GetDayName(DayOfWeek day) =>
        CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(day);

    private string FormatTime(TimeSpan ts)
    {
        return ts.ToString(@"hh\:mm");
    }
}
