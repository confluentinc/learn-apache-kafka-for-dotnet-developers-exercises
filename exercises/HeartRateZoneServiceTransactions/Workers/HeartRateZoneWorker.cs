using Confluent.Kafka;
using HeartRateZoneService.Domain;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
    private readonly ILogger<HeartRateZoneWorker> _logger;

    private readonly IConsumer<String, Biometrics> _consumer;

    private const string BiometricsImportedTopicName = "BiometricsImported";

    public HeartRateZoneWorker(ILogger<HeartRateZoneWorker> logger, IConsumer<string, Biometrics> consumer)
    {
        _logger = logger;
        _consumer = consumer;
        logger.LogInformation("HeartRateZoneWorker is Active.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        _consumer.Subscribe(BiometricsImportedTopicName);

        while(!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);

            await HandleMessage(result.Message.Value, stoppingToken);
        }

        _consumer.Close();  
    }   

    protected virtual async Task HandleMessage(Biometrics biometrics,CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received message: {biometrics.DeviceId}, MaxHeartRate: {biometrics.MaxHeartRate}, count: {biometrics.HeartRates.Count}");
    
        await Task.CompletedTask;
    } 
}
