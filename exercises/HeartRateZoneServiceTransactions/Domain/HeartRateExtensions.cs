using System;

namespace HeartRateZoneService.Domain
{
    public static class HeartRateZoneExtensions
    {
        private static double Zone1Threshold = 0.5;
        private static double Zone2Threshold = 0.6;
        private static double Zone3Threshold = 0.7;
        private static double Zone4Threshold = 0.8;
        private static double Zone5Threshold = 0.9;

        public static HeartRateZone GetHeartRateZone(this HeartRate heartRate, int maxHeartRate)
        {
            var percentage = (Double)heartRate.Value / (Double)maxHeartRate;

            var zone = HeartRateZone.None;

            if (percentage >= Zone5Threshold)
            {
                zone = HeartRateZone.Zone5;
            }
            else if (percentage >= Zone4Threshold)
            {
                zone = HeartRateZone.Zone4;
            }
            else if (percentage >= Zone3Threshold)
            {
                zone = HeartRateZone.Zone3;
            }
            else if (percentage >= Zone2Threshold)
            {
                zone = HeartRateZone.Zone2;
            }
            else if (percentage >= Zone1Threshold)
            {
                zone = HeartRateZone.Zone1;
            }

            return zone;
        }
    } 
}