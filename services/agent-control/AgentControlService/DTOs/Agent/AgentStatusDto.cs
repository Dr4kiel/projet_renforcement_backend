using System.Text.Json.Serialization;

namespace AgentControlService.DTOs.Agent;

public class AgentStatusDto
{
    [JsonPropertyName("line_id")]
    public string LineId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "running" | "stopped"

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
