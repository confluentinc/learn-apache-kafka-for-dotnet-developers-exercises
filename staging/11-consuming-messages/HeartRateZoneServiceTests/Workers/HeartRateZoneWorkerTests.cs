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
    private Mock<IConsumer<String, Biometrics>> _mockConsumer;
    private TestableWorker _worker;

    class TestableWorker : HeartRateZoneWorker
    {
        public bool SkipMessageProcessing { get; set; }
        public bool ThrowException { get; set; }
        public int MessagesHandled { get; set; }

        public TestableWorker(
            IConsumer<string, Biometrics> consumer,
            ILogger<HeartRateZoneWorker> logger
            ) : base(consumer, logger)
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
        _mockConsumer = new Mock<IConsumer<String, Biometrics>>();
        _cancellationToken = new CancellationTokenSource();

        _mockConsumer.Setup(consumer => consumer.Assignment)
            .Returns(new List<TopicPartition>());

        _worker = new TestableWorker(_mockConsumer.Object, _logger);
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
}
