# Apache Kafka for .NET Developers - Cheatsheet

## Basic Configuration

```json
{
	"Producer": {
		"BootstrapServers": "<Bootstrap_Servers>",
		"ClientId": "<Application_Name>",
		"SecurityProtocol": "SaslSsl",
		"SaslMechanism": "PLAIN",
		"SaslUsername": "<Kafka_API_Key>",
		"SaslPassword": "<Kafka_API_Secret>",
		"Acks": "All",
		"MessageTimeoutMs": 300000,
		"BatchNumMessages": 10000,
		"LingerMs": 5,
		"CompressionType": "Gzip",
	},
	"Consumer": {
		"BootstrapServers": "<Bootstrap_Servers>",
		"ClientId": "<Application_Name>",
		"SecurityProtocol": "SaslSsl",
		"SaslMechanism": "PLAIN",
		"SaslUsername": "<Kafka_API_Key>",
		"SaslPassword": "<Kafka_API_Secret>",
		"GroupId": "<Application_Group_Id>",
    	"AutoOffsetReset": "Earliest",
    	"EnableAutoCommit": true,
		"AutoCommitIntervalMs": 5000,
		"EnableAutoOffsetStore": false,
  	},
	"SchemaRegistry": {
		"URL": "<Schema_Registry_URL>",
		"BasicAuthCredentialsSource": "UserInfo",
		"BasicAuthUserInfo": "<SR_API_Key>:<SR_API_Secret>"
	}
}
```

## Loading the Configuration

```csharp
var kafkaProducerConfig = builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Producer"));
var kafkaConsumerConfig = builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Consumer"));
var schemaRegistryConfig = builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));
```

## Building a Producer

```csharp
var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

new ProducerBuilder<DeviceId, Biometrics>(kafkaProducerConfig)
	.SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistry))
	.SetKeySerializer(new JsonSerializer<DeviceId>(schemaRegistry)) 
	.Build();
```

## Producing Messages

```csharp
var message = new Message<DeviceId, Biometrics> {
	Key = deviceId,
	Value = biometrics
};
var result = await producer.ProduceAsync("TopicName", message);
```

## Building a Consumer

```csharp
var consumer = new ConsumerBuilder<DeviceId, Biometrics>(kafkaConsumerConfig)
	.SetKeyDeserializer(new JsonDeserializer<DeviceId>().AsSyncOverAsync())
	.SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
	.Build();
```

## Consuming Messages

```csharp
consumer.Subscribe("TopicName");

while(!cancellationToken.IsCancellationRequested)
{
    var result = consumer.Consume(cancellationToken);
    var deviceId = result.Message.Key;
    var biometrics = result.Message.Value;    
}
```

## Manual Offset Storage

```csharp
var result = consumer.Consume(stoppingToken);
...
consumer.StoreOffset(result);	
```

## Manual Commits

```csharp
var result = consumer.Consume(stoppingToken);
...
consumer.Commit();
```

## Transactions

```csharp
producer.InitTransactions(timeout);

while(!cancellationToken.IsCancellationRequested)
{
	var message = consumer.Consume(cancellationToken);
	
	producer.BeginTransaction();
	
	var offsets = consumer.Assignment.Select(topicPartition => new TopicPartitionOffset(topicPartition, consumer.Position(topicPartition)))
	
	producer.SendOffsetsToTransaction(offsets, consumer.ConsumerGroupMetadata, timeout);
	
	try 
	{
		var newMessage = ...
		await producer.ProduceAsync("TopicName", newMessage, cancellationToken);
	}
	catch (Exception ex)
	{
		producer.AbortTransaction();
	}
	
	producer.CommitTransaction();
}
```