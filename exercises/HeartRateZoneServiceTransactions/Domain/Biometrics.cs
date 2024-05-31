using System;
namespace HeartRateZoneService.Domain;

public class HeartRate
{
    public DateTime DateTime { get; }
    public int Value { get; }

    public HeartRate(int value, DateTime dateTime)
    {
        Value = value;
        DateTime = dateTime;
    }
}

public class Biometrics
{
    public Guid DeviceId { get; }
    public List<HeartRate> HeartRates { get; }
    public int MaxHeartRate { get; }

    public Biometrics(Guid deviceId, List<HeartRate> heartRates, int maxHeartRate)
    {
        DeviceId = deviceId;
        HeartRates = heartRates;
        MaxHeartRate = maxHeartRate;
    }
}