
using ClientGateway.Application;
using ClientGateway.Constants;
using ClientGateway.Domain;
using Confluent.Kafka;

namespace ClientGateway.Infrastructure;

public class ProducerService : IProducerService
{
    private readonly IProducer<String, BioMetrics> _producer;

    public ProducerService(IProducer<String, BioMetrics> producer)
    {
        _producer = producer;
    }

    public async Task ProduceAsync(BioMetrics metrics)
    {
        var result = await _producer.ProduceAsync(KafkaConstants.BiometricsImportedTopicName, new Message<String, BioMetrics>
        {
            Key = metrics.DeviceId.ToString(),
            Value = metrics
        });

        _producer.Flush();
    }
}