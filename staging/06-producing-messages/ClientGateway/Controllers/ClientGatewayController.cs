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
    private readonly ILogger<ClientGatewayController> _logger;

    public ClientGatewayController(ILogger<ClientGatewayController> logger)
    {
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
}



