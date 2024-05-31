using System;

namespace ClientGateway.Domain
{

    public class BioMetrics
    {  
        public BioMetrics(Guid deviceId, List<HeartRate> heartRates, List<StepCount> stepCounts, int maxHeartRate) 
        {
            this.DeviceId = deviceId;
            this.HeartRates = heartRates;
            this.StepCounts = stepCounts;
            this.MaxHeartRate = maxHeartRate;
        }
        public Guid DeviceId {get;}
        public List<HeartRate> HeartRates { get;}
        public List<StepCount> StepCounts { get; }
        public int MaxHeartRate { get; } 
  
    }
}