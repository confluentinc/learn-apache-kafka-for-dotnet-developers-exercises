using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
    private readonly ILogger<HeartRateZoneWorker> _logger;

    public HeartRateZoneWorker(ILogger<HeartRateZoneWorker> logger)
    {
        _logger = logger;
        logger.LogInformation("HeartRateZoneWorker is Active.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000);
    }   
}
