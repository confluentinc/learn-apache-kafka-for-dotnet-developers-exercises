using HeartRateZoneService.Domain;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
    private readonly ILogger<HeartRateZoneWorker> _logger;
    private readonly IConsumer<String, Biometrics> _consumer;
    private readonly IProducer<String, HeartRateZoneReached> _producer;

    private const String BiometricsImportedTopicName = "BiometricsImported";
    private const String HeartRateZoneReachedTopicName = "HeartRateZoneReached";
    private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public HeartRateZoneWorker(IConsumer<String, Biometrics> consumer, IProducer<String, HeartRateZoneReached> producer, ILogger<HeartRateZoneWorker> logger)
    {
        _logger = logger;
        _consumer = consumer;
        _producer = producer;

        logger.LogInformation("HeartRateZoneWorker is Active.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer.InitTransactions(DefaultTimeout);
        _consumer.Subscribe(BiometricsImportedTopicName);

        while(!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);

            await HandleMessage(result.Message.Value, stoppingToken);
        }

        _consumer.Close();
    }

    protected virtual async Task HandleMessage(Biometrics biometrics, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Message Received: " + biometrics.DeviceId);

        var offsets = _consumer.Assignment.Select(topicPartition =>
            new TopicPartitionOffset(
                topicPartition,
                _consumer.Position(topicPartition)
            )
        );

        _producer.BeginTransaction();
        _producer.SendOffsetsToTransaction(offsets, _consumer.ConsumerGroupMetadata, DefaultTimeout);

        try
        {
            await Task.WhenAll(biometrics.HeartRates
                .Where(hr => hr.GetHeartRateZone(biometrics.MaxHeartRate) != HeartRateZone.None)
                .Select(hr =>
                {
                    var zone = hr.GetHeartRateZone(biometrics.MaxHeartRate);

                    var heartRateZoneReached = new HeartRateZoneReached(
                        biometrics.DeviceId,
                        zone,
                        hr.DateTime,
                        hr.Value,
                        biometrics.MaxHeartRate
                    );

                    var message =  new Message<String, HeartRateZoneReached>
                    {
                        Key = biometrics.DeviceId.ToString(),
                        Value = heartRateZoneReached
                    };

                    return _producer.ProduceAsync(HeartRateZoneReachedTopicName, message, stoppingToken);
                })); 

            _producer.CommitTransaction();
        }
        catch (Exception ex)
        {
            _producer.AbortTransaction();
            throw new Exception("Transaction Failed", ex);
        }
    }
}
