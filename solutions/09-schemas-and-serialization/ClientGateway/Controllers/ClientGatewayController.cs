using ClientGateway.Domain;
using System.Diagnostics.Metrics;
using System.Net;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{
    private const string BiometricsImportedTopicName = "BiometricsImported";

    private readonly ILogger<ClientGatewayController> _logger;
    private readonly IProducer<String, Biometrics> _producer;

    public ClientGatewayController(IProducer<String, Biometrics> producer, ILogger<ClientGatewayController> logger)
    {
        _producer = producer;
        _logger = logger;
        logger.LogInformation("ClientGatewayController is Active.");
    }

    [HttpGet("Hello")]
    [ProducesResponseType(typeof(String), (int)HttpStatusCode.OK)]
    public String Hello()
    {
        _logger.LogInformation("Hello World");
        return "Hello World";
    }

    [HttpPost("Biometrics")]
    [ProducesResponseType(typeof(Biometrics), (int)HttpStatusCode.Accepted)]
    public async Task<AcceptedResult> RecordMeasurements(Biometrics metrics)
    {
        _logger.LogInformation("Accepted biometrics");

        var message = new Message<String, Biometrics>
        {
            Key = metrics.DeviceId.ToString(),
            Value = metrics
        };

        var result = await _producer.ProduceAsync(BiometricsImportedTopicName, message);

        _producer.Flush();

        return Accepted(result.Value);

    }
}



