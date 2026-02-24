using AgentControlService.DTOs.Agent;

namespace AgentControlService.Services.Interfaces;

public interface IRabbitMQPublisher
{
    Task PublishAgentCommandAsync(string lineId, AgentCommandDto command);
}
