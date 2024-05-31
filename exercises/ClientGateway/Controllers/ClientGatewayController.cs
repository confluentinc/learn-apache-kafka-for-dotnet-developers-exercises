using System.Net;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using ClientGateway.Domain;
using ClientGateway.Application;


namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{ 
    private readonly IProducer<String, BioMetrics> _producer;
    private readonly ILogger<ClientGatewayController> _logger;
    private readonly IProducerService _producerService;


    public ClientGatewayController(IProducer<String, BioMetrics> producer, ILogger<ClientGatewayController> logger, IProducerService producerService)
    {
        _producer = producer;
        _logger = logger;
        _producerService = producerService;

        logger.LogInformation("ClientGatewayController is Active.");
    } 

    [HttpPost("Biometrics")]
    [ProducesResponseType(typeof(BioMetrics), (int)HttpStatusCode.Accepted)]
    public async Task<AcceptedResult> RecordMeasurements(BioMetrics  metrics)
    {
        _logger.LogInformation("Accepted biometrics");

          await _producerService.ProduceAsync(metrics);
       

        return Accepted("", metrics);
        }
}



