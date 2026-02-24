using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using AgentControlService.DTOs.Agent;
using AgentControlService.Services.Interfaces;

namespace AgentControlService.Services;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private const string CommandsExchange = "agent.commands";

    public RabbitMQPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: CommandsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public async Task PublishAgentCommandAsync(string lineId, AgentCommandDto command)
    {
        var message = new
        {
            line_id = lineId,
            command = command.Command,
            timestamp = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var routingKey = $"line.{lineId}";

        await _lock.WaitAsync();
        try
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: CommandsExchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
