using System.Text.Json.Serialization;

namespace AgentControlService.DTOs.Common;

public class ApiErrorResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiErrorResponse Create(int statusCode, string message, Dictionary<string, string[]>? errors = null)
        => new() { StatusCode = statusCode, Message = message, Errors = errors };

    public static ApiErrorResponse BadRequest(string message, Dictionary<string, string[]>? errors = null)
        => Create(400, message, errors);

    public static ApiErrorResponse Unauthorized(string message = "Authentication required")
        => Create(401, message);

    public static ApiErrorResponse Forbidden(string message = "Access denied")
        => Create(403, message);

    public static ApiErrorResponse NotFound(string message = "Resource not found")
        => Create(404, message);

    public static ApiErrorResponse InternalServerError(string message = "An error occurred while processing your request")
        => Create(500, message);
}
