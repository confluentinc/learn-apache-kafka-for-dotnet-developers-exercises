using ClientGateway;
using ClientGateway.Controllers;
using ClientGateway.Domain;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));

builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();

    return new CachedSchemaRegistryClient(config.Value);
});

builder.Services.AddSingleton<IProducer<String, Biometrics>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    var schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();

    return new ProducerBuilder<String, Biometrics>(config.Value)
        .SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistry))
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();