namespace ClientGateway.Domain
{
    public class HeartRate
    {
        //constructor for HeartRate
        public HeartRate(int value, DateTime datetime)
        {
            this.Value = value;
            this.DateTime = datetime;
        }
        public int Value {get;}
        public DateTime DateTime { get; }
    }
}