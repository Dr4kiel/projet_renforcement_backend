using Microsoft.AspNetCore.SignalR;

namespace RealtimeService.Hubs;

public class NotificationsHub : Hub
{
    private readonly ILogger<NotificationsHub> _logger;

    public NotificationsHub(ILogger<NotificationsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to notifications: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from notifications: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows clients to acknowledge an alert/notification
    /// </summary>
    public async Task AcknowledgeNotification(int notificationId)
    {
        _logger.LogInformation("Notification {NotificationId} acknowledged by {ConnectionId}",
            notificationId, Context.ConnectionId);

        // Broadcast to all clients that this notification was acknowledged
        await Clients.Others.SendAsync("NotificationAcknowledged", notificationId, Context.ConnectionId);
    }
}
