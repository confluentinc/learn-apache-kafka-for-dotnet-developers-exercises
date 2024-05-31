using ClientGateway.Domain;

namespace ClientGateway.Application;
public interface IProducerService
{
    Task ProduceAsync(BioMetrics metrics);
}