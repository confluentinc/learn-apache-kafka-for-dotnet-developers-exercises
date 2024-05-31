using HeartRateZoneService;
using HeartRateZoneService.Workers;
using HeartRateZoneService.Domain;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Confluent.Kafka.SyncOverAsync;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    { 
        services.Configure<ConsumerConfig>(hostContext.Configuration.GetSection("Consumer"));
        
        services.AddSingleton(options => 
                {
                    var config =  options.GetRequiredService<IOptions<ConsumerConfig>>();   

                    return new ConsumerBuilder<String,Biometrics>(config.Value) 
                        .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
                        .Build();
                }       
        ); 

        services.AddHostedService<HeartRateZoneWorker>();
    })
    .Build();
 
await host.RunAsync();
 