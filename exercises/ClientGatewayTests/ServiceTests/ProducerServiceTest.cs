using ClientGateway.Constants;
using ClientGateway.Domain;
using ClientGateway.Infrastructure;
using Confluent.Kafka;
using Moq;

public class ProducerServiceTests
{
    [Test]
    public async Task ProduceAsync_ShouldProduceMessage()
    {
        // Arrange
        var metrics = new BioMetrics(Guid.NewGuid(), new List<HeartRate>(), new List<StepCount>(), 123);

        var mockProducer = new Mock<IProducer<string, BioMetrics>>();
        var producerService = new ProducerService(mockProducer.Object);

        // Act
        await producerService.ProduceAsync(metrics);

        // Assert
        mockProducer.Verify(p => p.ProduceAsync(KafkaConstants.BiometricsImportedTopicName, It.Is<Message<string, BioMetrics>>(
            m => m.Key == metrics.DeviceId.ToString() && m.Value == metrics), It.IsAny<CancellationToken>()), Times.Once);
 
    }
}