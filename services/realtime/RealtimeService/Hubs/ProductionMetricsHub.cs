using Microsoft.AspNetCore.SignalR;

namespace RealtimeService.Hubs;

public class ProductionMetricsHub : Hub
{
    private readonly ILogger<ProductionMetricsHub> _logger;

    public ProductionMetricsHub(ILogger<ProductionMetricsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows clients to subscribe to specific line metrics
    /// </summary>
    public async Task SubscribeToLine(int lineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"line_{lineId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to line {LineId}", Context.ConnectionId, lineId);
    }

    /// <summary>
    /// Allows clients to unsubscribe from specific line metrics
    /// </summary>
    public async Task UnsubscribeFromLine(int lineId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"line_{lineId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from line {LineId}", Context.ConnectionId, lineId);
    }
}
