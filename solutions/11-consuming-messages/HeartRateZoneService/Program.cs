using HeartRateZoneService;
using HeartRateZoneService.Workers;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Confluent.Kafka.SyncOverAsync;
using HeartRateZoneService.Domain;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<ConsumerConfig>(hostContext.Configuration.GetSection("Consumer"));

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ConsumerConfig>>();

            return new ConsumerBuilder<String, Biometrics>(config.Value)
                .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
                .Build();
        });

        services.AddHostedService<HeartRateZoneWorker>();
    })
    .Build();

await host.RunAsync();

