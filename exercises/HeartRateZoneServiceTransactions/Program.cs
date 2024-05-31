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
        services.Configure<ProducerConfig>(hostContext.Configuration.GetSection("Producer"));
        services.Configure<ConsumerConfig>(hostContext.Configuration.GetSection("Consumer"));
        services.Configure<SchemaRegistryConfig>(hostContext.Configuration.GetSection("SchemaRegistry"));

        services.AddSingleton(options => 
                {
                    var config =  options.GetRequiredService<IOptions<ConsumerConfig>>();   

                    return new ConsumerBuilder<String,Biometrics>(config.Value) 
                        .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
                        .Build();
                }       
        );

        services.AddSingleton<ISchemaRegistryClient>(options => 
        {
            var config = options.GetRequiredService<IOptions<SchemaRegistryConfig>>();

            return new CachedSchemaRegistryClient(config.Value);
        });  

        services.AddSingleton<IProducer<string, HeartRateZoneReached>>(options => 
        {
            var config = options.GetRequiredService<IOptions<ProducerConfig>>();
            var schemaRegistry = options.GetRequiredService<ISchemaRegistryClient>();

            return new ProducerBuilder<String,HeartRateZoneReached>(config.Value)
                .SetValueSerializer(new JsonSerializer<HeartRateZoneReached>(schemaRegistry))
                .Build();
        });

        services.AddHostedService<HeartRateZoneWorker>();
    })
    .Build();
 
await host.RunAsync();

/*  
Use services where you would have used builder.Services.
Use hostContext.Configuration where you would have used builder.Configuration.
Hint: What is the name of the section we are loading?
Register an instance of an IConsumer<String, Biometrics> (using services.AddSingleton).
Retrieve the ConsumerConfig.
Create a new ConsumerBuilder<String, Biometrics>.
Call SetValueDeserializer and pass it a new JsonDeserializer<Biometrics>().AsSyncOverAsync()
Call Build to build the consumer and return it.

*/