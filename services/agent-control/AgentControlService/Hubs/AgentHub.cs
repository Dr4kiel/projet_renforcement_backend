using Microsoft.AspNetCore.SignalR;

namespace AgentControlService.Hubs;

public class AgentHub : Hub
{
    public async Task SubscribeToLine(string lineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"line_{lineId}");
    }

    public async Task UnsubscribeFromLine(string lineId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"line_{lineId}");
    }
}
