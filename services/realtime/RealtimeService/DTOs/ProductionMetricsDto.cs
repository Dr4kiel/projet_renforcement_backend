namespace RealtimeService.DTOs;

public class ProductionMetricsDto
{
    public int LineId { get; set; }
    public string LineName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, decimal> Metrics { get; set; } = new();
    public LineStatusDto Status { get; set; } = new();
}

public class LineStatusDto
{
    public string? CurrentOf { get; set; }
    public string? CurrentProduct { get; set; }
    public int QteProduite { get; set; }
    public int QteTotale { get; set; }
    public string? NextOf { get; set; }
    public bool IsChangement { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // info, warning, error
    public DateTime Timestamp { get; set; }
    public int? LineId { get; set; }
    public string? LineName { get; set; }
}
