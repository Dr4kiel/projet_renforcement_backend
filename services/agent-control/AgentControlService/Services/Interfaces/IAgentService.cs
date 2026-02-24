namespace AgentControlService.Services.Interfaces;

public interface IAgentService
{
    Task StartLineAsync(string lineId);
    Task StopLineAsync(string lineId);
}
