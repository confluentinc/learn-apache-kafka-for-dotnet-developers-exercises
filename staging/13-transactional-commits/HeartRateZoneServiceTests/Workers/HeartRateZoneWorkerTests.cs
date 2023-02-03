using Confluent.Kafka;
using HeartRateZoneService.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using HeartRateZoneService.Workers;
using Microsoft.AspNetCore.Mvc;
using HeartRateZoneService;
using static Confluent.Kafka.ConfigPropertyNames;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace HeartRateZoneServiceTests.Workers;

public class HeartRateZoneWorkerTests
{
    private Logger<HeartRateZoneWorker> _logger;
    private CancellationTokenSource _cancellationToken;
    private Mock<IProducer<String, HeartRateZoneReached>> _mockProducer;
    private Mock<IConsumer<String, Biometrics>> _mockConsumer;
    private TestableWorker _worker;

    class TestableWorker : HeartRateZoneWorker
    {
        public bool SkipMessageProcessing { get; set; }
        public bool ThrowException { get; set; }
        public int MessagesHandled { get; set; }

        public TestableWorker(
            IConsumer<string, Biometrics> consumer,
            IProducer<string, HeartRateZoneReached> producer,
            ILogger<HeartRateZoneWorker> logger
            ) : base(consumer, producer, logger)
        {
        }

        public async Task TestExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);
        }

        public async Task TestHandleMessage(Biometrics biometrics, CancellationToken stoppingToken)
        {
            await HandleMessage(biometrics, stoppingToken);
        }

        protected override async Task HandleMessage(Biometrics biometrics, CancellationToken stoppingToken)
        {
            MessagesHandled++;

            if (ThrowException)
            {
                throw new Exception("Boom");
            }

            if (!SkipMessageProcessing)
            {
                await base.HandleMessage(biometrics, stoppingToken);
            }
        }
    }

    [SetUp]
    public void Setup()
    {
        _logger = new Logger<HeartRateZoneWorker>(new LoggerFactory());
        _mockProducer = new Mock<IProducer<String, HeartRateZoneReached>>();
        _mockConsumer = new Mock<IConsumer<String, Biometrics>>();
        _cancellationToken = new CancellationTokenSource();

        _mockConsumer.Setup(consumer => consumer.Assignment)
            .Returns(new List<TopicPartition>());

        _worker = new TestableWorker(_mockConsumer.Object, _mockProducer.Object, _logger);
    }

    [Test]
    public async Task ExecuteAsync_ShouldConsumeMessagesUntilCanceled()
    {
        var message = TestHelpers.CreateMessage();
        _worker.SkipMessageProcessing = true;

        const int maxCalls = 5;
        var calls = 0;

        _mockConsumer.Setup(consumer => consumer.Consume(_cancellationToken.Token))
            .Returns(new ConsumeResult<string, Biometrics> { Message = message })
            .Callback(() => { if (++calls >= maxCalls) _cancellationToken.Cancel(); } );

        await _worker.TestExecuteAsync(_cancellationToken.Token);

        _mockProducer.Verify(producer => producer.InitTransactions(It.IsAny<TimeSpan>()), Times.Once());
        _mockConsumer.Verify(consumer => consumer.Subscribe("BiometricsImported"), Times.Once());
        _mockConsumer.Verify(consumer => consumer.Close(), Times.Once());

        _mockConsumer.Verify(consumer => consumer.Consume(_cancellationToken.Token), Times.Exactly(maxCalls));
        Assert.That(_worker.MessagesHandled, Is.EqualTo(maxCalls));
    }

    [Test]
    public void ExecuteAsync_ShouldFail_IfHandlingTheMessageFails()
    {
        var message = TestHelpers.CreateMessage();
        _worker.SkipMessageProcessing = true;
        _worker.ThrowException = true;

        _mockConsumer.Setup(consumer => consumer.Consume(_cancellationToken.Token))
            .Returns(new ConsumeResult<string, Biometrics> { Message = message })
            .Callback(() => _cancellationToken.Cancel());

        Assert.ThrowsAsync<Exception>(() =>
            _worker.TestExecuteAsync(_cancellationToken.Token)
        );
    }

    [Test]
    public async Task HandleMessage_ShouldExecuteAKafkaTransaction()
    {
        var biometrics = TestHelpers.CreateBiometrics();
        var metadata = new Mock<IConsumerGroupMetadata>();

        _mockConsumer.Setup(consumer => consumer.Assignment)
            .Returns(new List<TopicPartition>());

        _mockConsumer.Setup(consumer => consumer.ConsumerGroupMetadata)
            .Returns(metadata.Object);

        await _worker.TestHandleMessage(biometrics, _cancellationToken.Token);

        _mockProducer.Verify(producer => producer.BeginTransaction(), Times.Once());
        _mockProducer.Verify(producer => producer.CommitTransaction(), Times.Once());
        _mockProducer.Verify(producer =>
            producer.SendOffsetsToTransaction(
                It.IsAny<IEnumerable<TopicPartitionOffset>>(),
                metadata.Object,
                It.IsAny<TimeSpan>()
            ),
            Times.Once()
        );
    }

    [Test]
    public async Task HandleMessage_ShouldNotProduceAnyMessages_IfThereAreNoHeartRates()
    {
        var biometrics = TestHelpers.CreateBiometrics(
            TestHelpers.CreateDeviceId(),
            new List<HeartRate>(),
            100
        );

        await _worker.TestHandleMessage(biometrics, _cancellationToken.Token);

        _mockProducer.Verify(producer => producer.ProduceAsync(
            It.IsAny<String>(),
            It.IsAny<Message<String, HeartRateZoneReached>>(),
            It.IsAny<CancellationToken>()
        ), Times.Never());
    }

    [Test]
    public async Task HandleMessage_ShouldExpectedProduceMessages_ForAnyHeartRatesThatExceedTheThresholds()
    {
        var dateTime = DateTime.Now;
        var maxHeartRate = 100;

        var heartRates = new List<int> { 0, 49, 50, 60, 70, 30, 80, 90, 100 }
            .Select(value => new HeartRate(value, dateTime))
            .ToList();

        var biometrics = TestHelpers.CreateBiometrics(heartRates, maxHeartRate);

        var actualMessages = new List<Message<String, HeartRateZoneReached>>();

        _mockProducer.Setup(producer => producer.ProduceAsync(
            It.IsAny<String>(),
            It.IsAny<Message<String, HeartRateZoneReached>>(),
            It.IsAny<CancellationToken>())
        ).Callback<String, Message<String, HeartRateZoneReached>, CancellationToken>((topic, message, token) =>
            {
                Assert.That(message.Key, Is.EqualTo(biometrics.DeviceId.ToString()));
                Assert.That(message.Value.DeviceId, Is.EqualTo(biometrics.DeviceId));
                Assert.That(message.Value.DateTime, Is.EqualTo(dateTime));
                Assert.That(message.Value.MaxHeartRate, Is.EqualTo(maxHeartRate));

                actualMessages.Add(message);
            }
        );

        await _worker.TestHandleMessage(biometrics, _cancellationToken.Token);

        _mockProducer.Verify(producer => producer.ProduceAsync(
            "HeartRateZoneReached",
            It.IsAny<Message<String, HeartRateZoneReached>>(),
            _cancellationToken.Token
        ), Times.Exactly(6));

        var expectedHeartRates = heartRates.Where(hr => hr.Value >= 50).Select(hr => hr.Value);
        var actualHeartRates = actualMessages.Select(msg => msg.Value.HeartRate);

        Assert.That(actualHeartRates, Is.EquivalentTo(expectedHeartRates));
    }

    [Test]
    public void HandleMessage_ShouldFail_IfProducingAMessageFails()
    {
        var biometrics = TestHelpers.CreateBiometrics();

        _mockProducer.Setup(producer => producer.ProduceAsync(
            It.IsAny<String>(),
            It.IsAny<Message<String, HeartRateZoneReached>>(),
            It.IsAny<CancellationToken>())
        ).Returns(
            Task.FromException<DeliveryResult<String, HeartRateZoneReached>>(new Exception("Boom"))
        );

        Console.WriteLine("Begin");
        Assert.ThrowsAsync<Exception>(() =>
            _worker.TestHandleMessage(biometrics, _cancellationToken.Token)
        );

        _mockProducer.Verify(producer => producer.CommitTransaction(), Times.Never);
        _mockProducer.Verify(producer => producer.AbortTransaction(), Times.Once);
    }
}
