
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