namespace ClientGatewayTests.Controllers;

using Moq;
using Confluent.Kafka;
using ClientGateway;
using ClientGateway.Controllers;
using ClientGateway.Domain;
using Microsoft.Extensions.Logging;

public class ClientGatewayControllerTests
{
    private Logger<ClientGatewayController> _logger;
    private Mock<IProducer<String, Biometrics>> _mockProducer;
    private ClientGatewayController _controller;

    [SetUp]
    public void Setup()
    {
        _logger = new Logger<ClientGatewayController>(new LoggerFactory());
        _mockProducer = new Mock<IProducer<String, Biometrics>>();
        _controller = new ClientGatewayController(_mockProducer.Object, _logger);
    }

    [Test]
    public async Task RecordMeasurements_ShouldProduceTheExpectedMessage()
    {
        var expectedTopic = "BiometricsImported";
        var expectedMessage = TestHelpers.createMessage();

        _mockProducer.Setup(producer => producer.ProduceAsync(It.IsAny<String>(), It.IsAny<Message<String, Biometrics>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new DeliveryResult<String, Biometrics>
            {
                Message = expectedMessage
            }));

        var result = await _controller.RecordMeasurements(expectedMessage.Value);

        _mockProducer.Verify(producer => producer.ProduceAsync(
            expectedTopic,
            It.Is<Message<String, Biometrics>>(msg => msg.Key == expectedMessage.Key && msg.Value == expectedMessage.Value),
            It.IsAny<CancellationToken>()));

        _mockProducer.Verify(producer => producer.Flush(It.IsAny<CancellationToken>()), Times.Once());

        Assert.That(result.Value, Is.EqualTo(expectedMessage.Value));
    }

    [Test]
    public void RecordMeasurements_ShouldReturnAFailure_IfTheMessageProducerFails()
    {
        var expectedTopic = "BiometricsImported";
        var expectedMessage = TestHelpers.createMessage();

        _mockProducer.Setup(producer => producer.ProduceAsync(It.IsAny<String>(), It.IsAny<Message<String, Biometrics>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromException<DeliveryResult<String, Biometrics>>(new Exception("Boom")));

        Assert.ThrowsAsync<Exception>(() => _controller.RecordMeasurements(expectedMessage.Value));

        _mockProducer.Verify(producer => producer.ProduceAsync(
            expectedTopic,
            It.Is<Message<String, Biometrics>>(msg => msg.Key == expectedMessage.Key && msg.Value == expectedMessage.Value),
            It.IsAny<CancellationToken>()));
    }
}

