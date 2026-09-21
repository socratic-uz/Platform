namespace Domain.DTOs.Seating;

/// <summary>
/// Информация о выбранном посадочном месте на интерактивной схеме.
/// </summary>
public class SeatSelectionInfo
{
    public string SeatId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
}

/// <summary>
/// Информация о выбранном узле (стол/место) для дизайнера схемы зала.
/// </summary>
public class SelectedNodeInfo
{
    public string Id { get; set; } = string.Empty;
    public bool IsSeat { get; set; }
    public string SeatId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Price { get; set; }
}
