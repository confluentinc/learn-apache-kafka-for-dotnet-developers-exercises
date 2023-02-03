using System;
using Confluent.Kafka;
using HeartRateZoneService.Domain;
using HeartRateZoneService.Workers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;

namespace HeartRateZoneServiceTests.Domain;

public class HeartRateExtensionsTests
{
    [Test]
    public void GetHeartRateZone_ShouldReturnNoneLessThan50PercentOfMax()
    {
        var hrMinus1 = new HeartRate(-1, DateTime.Now);
        var hr0 = new HeartRate(0, DateTime.Now);
        var hr25 = new HeartRate(25, DateTime.Now);
        var hr49 = new HeartRate(49, DateTime.Now);
        var maxHr = 100;

        Assert.That(hrMinus1.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.None));
        Assert.That(hr0.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.None));
        Assert.That(hr25.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.None));
        Assert.That(hr49.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.None));
    }

    [Test]
    public void GetHeartRateZone_ShouldZone1_IfGreaterThan50PercentOfMax()
    {
        var hr50 = new HeartRate(50, DateTime.Now);
        var hr51 = new HeartRate(51, DateTime.Now);
        var hr59 = new HeartRate(59, DateTime.Now);
        var maxHr = 100;

        Assert.That(hr50.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone1));
        Assert.That(hr51.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone1));
        Assert.That(hr59.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone1));
    }

    [Test]
    public void GetHeartRateZone_ShouldZone2_IfGreaterThan60PercentOfMax()
    {
        var hr60 = new HeartRate(60, DateTime.Now);
        var hr61 = new HeartRate(61, DateTime.Now);
        var hr69 = new HeartRate(69, DateTime.Now);
        var maxHr = 100;

        Assert.That(hr60.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone2));
        Assert.That(hr61.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone2));
        Assert.That(hr69.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone2));
    }

    [Test]
    public void GetHeartRateZone_ShouldZone3_IfGreaterThan70PercentOfMax()
    {
        var hr70 = new HeartRate(70, DateTime.Now);
        var hr71 = new HeartRate(71, DateTime.Now);
        var hr79 = new HeartRate(79, DateTime.Now);
        var maxHr = 100;

        Assert.That(hr70.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone3));
        Assert.That(hr71.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone3));
        Assert.That(hr79.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone3));
    }

    [Test]
    public void GetHeartRateZone_ShouldZone4_IfGreaterThan80PercentOfMax()
    {
        var hr80 = new HeartRate(80, DateTime.Now);
        var hr81 = new HeartRate(81, DateTime.Now);
        var hr89 = new HeartRate(89, DateTime.Now);
        var maxHr = 100;

        Assert.That(hr80.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone4));
        Assert.That(hr81.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone4));
        Assert.That(hr89.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone4));
    }

    [Test]
    public void GetHeartRateZone_ShouldZone5_IfGreaterThan90PercentOfMax()
    {
        var hr90 = new HeartRate(90, DateTime.Now);
        var hr91 = new HeartRate(91, DateTime.Now);
        var hr99 = new HeartRate(99, DateTime.Now);
        var hr100 = new HeartRate(100, DateTime.Now);
        var hr101 = new HeartRate(101, DateTime.Now);
        var hr125 = new HeartRate(125, DateTime.Now);
        var maxHr = 100;

        Assert.That(hr90.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
        Assert.That(hr91.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
        Assert.That(hr99.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
        Assert.That(hr100.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
        Assert.That(hr101.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
        Assert.That(hr125.GetHeartRateZone(maxHr), Is.EqualTo(HeartRateZone.Zone5));
    }
}