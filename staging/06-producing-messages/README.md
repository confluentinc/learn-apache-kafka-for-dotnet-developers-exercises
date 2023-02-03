# Building Event Streaming Applications in .Net

This application is designed to be used in conjunction with the Confluent Developer course.

## Compile

To compile your code, execute the following:

```bash
dotnet msbuild Fitness.sln
```

## Test

To run your tests, execute the following:

```bash
dotnet test Fitness.sln
```

## Execute

The application consists of two microservices. Each will need to be executed in a separate terminal.

To execute the application, first, run the **Compile** and **Test** steps above.

To execute the ClientGateway, run the following command:

```bash
cd ClientGateway
dotnet run 
```

To execute the HeartRateZoneService, run the following command:

```bash
cd HeartRateZoneService
dotnet run
```