using System;
using ClientGateway;
using ClientGateway.Domain;
using Confluent.Kafka;

namespace ClientGatewayTests;

public class TestHelpers
{
    private static Random rnd = new Random();

    public static Biometrics createBiometrics()
    {
        var heartRates = new List<HeartRate>();

        for(int i = 0; i < rnd.Next(9) + 1; i++)
        {
            heartRates.Add(new HeartRate(rnd.Next(50, 200), DateTime.Now));
        }

        var stepCounts = new List<StepCount>();

        for (int i = 0; i < rnd.Next(9) + 1; i++)
        {
            heartRates.Add(new HeartRate(rnd.Next(0, 500), DateTime.Now));
        }

        return new Biometrics(
            Guid.NewGuid(),
            heartRates,
            stepCounts,
            rnd.Next(150, 200)
        );
    }

    public static Message<String, Biometrics> createMessage()
    {
        var biometrics = createBiometrics();

        return new Message<String, Biometrics>
        {
            Key = biometrics.DeviceId.ToString(),
            Value = biometrics
        };
    }
}

