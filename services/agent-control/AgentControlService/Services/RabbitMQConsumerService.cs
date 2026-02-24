using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AgentControlService.DTOs.Agent;
using AgentControlService.Hubs;

namespace AgentControlService.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<AgentHub> _hubContext;
    private IConnection? _connection;
    private IModel? _channel;
    private const string StatusExchange = "agent.status";

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IConfiguration configuration,
        IHubContext<AgentHub> hubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: StatusExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false
        );

        // Queue exclusive et auto-delete : liée au cycle de vie de ce service
        var queueName = _channel.QueueDeclare(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true
        ).QueueName;

        _channel.QueueBind(queue: queueName, exchange: StatusExchange, routingKey: string.Empty);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceivedAsync;

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        stoppingToken.Register(() =>
        {
            _channel?.Close();
            _connection?.Close();
        });

        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var json = Encoding.UTF8.GetString(e.Body.ToArray());
            var status = JsonSerializer.Deserialize<AgentStatusDto>(json);

            if (status is null) return;

            _logger.LogInformation(
                "Agent status received: Line={LineId} Status={Status} Success={Success}",
                status.LineId, status.Status, status.Success);

            await _hubContext.Clients.All.SendAsync("AgentStatusUpdated", status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agent status message");
        }
    }
}
