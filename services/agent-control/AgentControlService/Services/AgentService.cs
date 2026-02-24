using AgentControlService.DTOs.Agent;
using AgentControlService.Services.Interfaces;

namespace AgentControlService.Services;

public class AgentService : IAgentService
{
    private readonly IRabbitMQPublisher _publisher;

    public AgentService(IRabbitMQPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task StartLineAsync(string lineId)
        => _publisher.PublishAgentCommandAsync(lineId, new AgentCommandDto { Command = "start" });

    public Task StopLineAsync(string lineId)
        => _publisher.PublishAgentCommandAsync(lineId, new AgentCommandDto { Command = "stop" });
}
