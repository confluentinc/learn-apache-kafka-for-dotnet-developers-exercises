using ClientGateway;
using ClientGateway.Controllers;
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

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();

    return new ProducerBuilder<String, String>(config.Value)
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();