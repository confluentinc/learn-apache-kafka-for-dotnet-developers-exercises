using System;
using HeartRateZoneService;
using HeartRateZoneService.Domain;
using Confluent.Kafka;

namespace HeartRateZoneServiceTests;

public class TestHelpers
{
    private static Random rnd = new Random();

    public static Biometrics CreateBiometrics()
    {
        return CreateBiometrics(
            CreateDeviceId(),
            CreateHeartRates(),
            rnd.Next(0, 200)
        );
    }

    public static Biometrics CreateBiometrics(List<HeartRate> heartRates, int maxHeartRate)
    {
        return new Biometrics(
            CreateDeviceId(),
            heartRates,
            maxHeartRate
        );
    }

    public static Biometrics CreateBiometrics(Guid deviceId, List<HeartRate> heartRates, int maxHeartRate)
    {
        return new Biometrics(
            deviceId,
            heartRates,
            maxHeartRate
        );
    }

    public static Message<String, Biometrics> CreateMessage()
    {
        var biometrics = CreateBiometrics();

        return new Message<String, Biometrics>
        {
            Key = biometrics.DeviceId.ToString(),
            Value = biometrics
        };
    }

    public static List<HeartRate> CreateHeartRates()
    {
        var heartRates = new List<HeartRate>();

        for (int i = 0; i < rnd.Next(9) + 1; i++)
        {
            heartRates.Add(CreateHeartRate());
        }

        return heartRates;
    }

    public static Guid CreateDeviceId()
    {
        return Guid.NewGuid();
    }

    public static HeartRate CreateHeartRate()
    {
        return new HeartRate(rnd.Next(50, 200), DateTime.Now);
    }
}

