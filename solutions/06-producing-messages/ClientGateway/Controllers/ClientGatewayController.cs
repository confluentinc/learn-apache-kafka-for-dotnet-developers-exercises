﻿using System.Diagnostics.Metrics;
using System.Net;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{
    private const string BiometricsImportedTopicName = "RawBiometricsImported";

    private readonly IProducer<String, String> _producer;
    private readonly ILogger<ClientGatewayController> _logger;

    public ClientGatewayController(IProducer<String, String> producer, ILogger<ClientGatewayController> logger)
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
    [ProducesResponseType(typeof(String), (int)HttpStatusCode.Accepted)]
    public async Task<AcceptedResult> RecordMeasurements(String metrics)
    {
        _logger.LogInformation("Accepted biometrics");

        var result = await _producer.ProduceAsync(BiometricsImportedTopicName, new Message<String, String>
        {
            Value = metrics
        });

        _producer.Flush();

        return Accepted("", metrics);
    }
}



