namespace Web.UI.Models;

/// <summary>
/// Представляет команду управления удаленным рабочим столом (клики, движения мыши, ввод текста).
/// </summary>
public class RemoteControlCommand
{
    /// <summary>
    /// Тип события ("Click", "Move", "KeyDown", "KeyUp").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Положение X на экране в процентах (0.0 - 100.0).
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Положение Y на экране в процентах (0.0 - 100.0).
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Виртуальный код клавиши (для клавиатурного ввода).
    /// </summary>
    public int KeyCode { get; set; }
}
