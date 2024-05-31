using Confluent.Kafka;
using HeartRateZoneService.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using HeartRateZoneService.Workers;
using HeartRateZoneService.Constants; 

namespace HeartRateZoneServiceTests.Workers;

public class HeartRateZoneWorkerTests
{
    private Logger<HeartRateZoneWorker> _logger;
    private CancellationTokenSource _cancellationToken;
    private Mock<IConsumer<String, Biometrics>> _mockConsumer;
  
    [SetUp]
    public void Setup()
    {
        _logger = new Logger<HeartRateZoneWorker>(new LoggerFactory());
        _mockConsumer = new Mock<IConsumer<String, Biometrics>>();
        _cancellationToken = new CancellationTokenSource();

        _mockConsumer.Setup(consumer => consumer.Assignment)
            .Returns(new List<TopicPartition>()); 
        
    } 
	
    // [Test]
    // public async Task ExecuteAsync_ShouldConsumeAndHandleMessage()
    // {
    //     // Arrange
    //     var stoppingToken = new CancellationToken();
    //     var consumer = new Mock<IConsumer<string, BioMetrics>>();
    //     var worker = new HeartRateZoneWorker(consumer.Object);

    //     var message = new Message<string, BioMetrics>
    //     {
    //         Value = new BioMetrics(Guid.NewGuid(), new List<HeartRate>(), new List<StepCount>(), 123)
    //     };

    //     consumer.Setup(c => c.Consume(stoppingToken)).Returns(message);

    //     // Act
    //     await worker.ExecuteAsync(stoppingToken);

    //     // Assert
    //     consumer.Verify(c => c.Subscribe(KafkaConstants.BiometricsImportedTopicName), Times.Once);
    //     consumer.Verify(c => c.Consume(stoppingToken), Times.Once);
    //     consumer.Verify(c => c.Close(), Times.Once);       
    // }
 
}
