using System;
namespace ClientGateway.Domain;

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

public class StepCount
{
    public DateTime DateTime { get; }
    public int Value { get; }

    public StepCount(int value, DateTime dateTime)
    {
        Value = value;
        DateTime = dateTime;
    }
}

public class BioMetrics
{
    public Guid DeviceId { get; }
    public List<HeartRate> HeartRates { get; }
    public List<StepCount> StepCounts { get; }
    public int MaxHeartRate { get; }

    public BioMetrics(Guid deviceId, List<HeartRate> heartRates, List<StepCount> stepCounts, int maxHeartRate)
    {
        DeviceId = deviceId;
        HeartRates = heartRates;
        StepCounts = stepCounts;
        MaxHeartRate = maxHeartRate;
    }
}

