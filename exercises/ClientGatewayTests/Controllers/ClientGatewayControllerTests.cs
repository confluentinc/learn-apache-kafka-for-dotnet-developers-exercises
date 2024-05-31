using ClientGateway.Application;
using ClientGateway.Controllers;
using ClientGateway.Domain;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ClientGatewayTests.ControllerTests
{
    public class ClientGatewayControllerTest
    {
        private Mock<IProducer<string, BioMetrics>> _mockProducer;
        private Mock<ILogger<ClientGatewayController>> _mockLogger;
        private Mock<IProducerService> _mockProducerService;
        private ClientGatewayController _controller;

        [SetUp]
        public void Setup()
        {
            _mockProducer = new Mock<IProducer<string, BioMetrics>>();
            _mockLogger = new Mock<ILogger<ClientGatewayController>>();
            _mockProducerService = new Mock<IProducerService>();
            _controller = new ClientGatewayController(_mockProducer.Object, _mockLogger.Object, _mockProducerService.Object);
        } 

        [Test]
        public async Task RecordMeasurements_ReturnsAcceptedResult()
        {
            // Arrange
            var metrics = new BioMetrics(Guid.NewGuid(), null, null, 123);

            // Act
            var result = await _controller.RecordMeasurements(metrics) as AcceptedResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.Accepted));
            Assert.That(result.Location, Is.EqualTo(""));
            Assert.That(result.Value, Is.EqualTo(metrics)); 
            _mockProducerService.Verify(x => x.ProduceAsync(metrics), Times.Once);
        }

       
        [Test]
        public void RecordMeasurements_ShouldThrowException_WhenProduceAsyncFails()
        {
            // Arrange
            var metrics = new BioMetrics(Guid.NewGuid(), null, null, 123); // replace with actual instance
            _mockProducerService.Setup(p => p.ProduceAsync(metrics)).ThrowsAsync(new Exception());

            // Act & Assertd
            Assert.ThrowsAsync<Exception>(() => _controller.RecordMeasurements(metrics));
        }

    }
}