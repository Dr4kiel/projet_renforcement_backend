using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealtimeService.Data;
using RealtimeService.DTOs;
using RealtimeService.Hubs;

namespace RealtimeService.Services;

public class MetricsBroadcastService : BackgroundService
{
    private readonly ILogger<MetricsBroadcastService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<ProductionMetricsHub> _metricsHubContext;
    private readonly int _pollingIntervalSeconds;

    public MetricsBroadcastService(
        ILogger<MetricsBroadcastService> logger,
        IServiceProvider serviceProvider,
        IHubContext<ProductionMetricsHub> metricsHubContext,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _metricsHubContext = metricsHubContext;
        _pollingIntervalSeconds = configuration.GetValue<int>("SignalR:HistorianPollingIntervalSeconds", 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics Broadcast Service starting. Polling interval: {Interval}s", _pollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastMetricsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while broadcasting metrics");
            }

            await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Metrics Broadcast Service stopped");
    }

    private async Task BroadcastMetricsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RealtimeDbContext>();

        // Get all lines with their equipment and OFs
        var lines = await dbContext.Lines
            .Include(l => l.Equipment)
                .ThenInclude(e => e.Tags)
            .Include(l => l.OfEnCours)
            .Include(l => l.OfSuivant)
            .ToListAsync(cancellationToken);

        foreach (var line in lines)
        {
            try
            {
                // Get the latest metrics for this line's equipment tags
                var tagIds = line.Equipment.Tags.Select(t => t.Id).ToList();

                if (tagIds.Any())
                {
                    // Get the most recent historian data for each tag (within the last 30 seconds)
                    var cutoffTime = DateTime.UtcNow.AddSeconds(-30);

                    var recentHistorian = await dbContext.Historian
                        .Where(h => tagIds.Contains(h.TagNameId) && h.Timestamp >= cutoffTime)
                        .Include(h => h.TagNameNavigation)
                        .ToListAsync(cancellationToken);

                    var latestMetrics = recentHistorian
                        .GroupBy(h => h.TagNameId)
                        .Select(g => g.OrderByDescending(h => h.Timestamp).First())
                        .ToList();

                    // Build metrics dictionary
                    var metricsDict = latestMetrics
                        .Where(m => m != null)
                        .ToDictionary(
                            m => m!.TagNameNavigation.TagName,
                            m => m!.Value
                        );

                    // Build production metrics DTO
                    var metricsDto = new ProductionMetricsDto
                    {
                        LineId = line.Id,
                        LineName = line.Name,
                        Timestamp = DateTime.UtcNow,
                        Metrics = metricsDict,
                        Status = new LineStatusDto
                        {
                            CurrentOf = line.OfEnCours?.Of_,
                            CurrentProduct = line.OfEnCours?.Produit,
                            QteProduite = line.OfEnCours?.QteProduite ?? 0,
                            QteTotale = line.OfEnCours?.QteTotale ?? 0,
                            NextOf = line.OfSuivant?.Of_,
                            IsChangement = line.IsChangement
                        }
                    };

                    // Broadcast to all clients
                    await _metricsHubContext.Clients.All.SendAsync(
                        "ReceiveMetrics",
                        metricsDto,
                        cancellationToken);

                    // Also broadcast to line-specific group
                    await _metricsHubContext.Clients.Group($"line_{line.Id}").SendAsync(
                        "ReceiveLineMetrics",
                        metricsDto,
                        cancellationToken);

                    _logger.LogDebug("Broadcasted metrics for line {LineName} with {Count} metrics",
                        line.Name, metricsDict.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting metrics for line {LineName}", line.Name);
            }
        }
    }
}
