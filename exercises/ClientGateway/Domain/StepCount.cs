namespace ClientGateway.Domain
{
    public class StepCount
    {  
        //constructor for StepCount
        public StepCount(int value, DateTime datetime)
        {
            this.Value = value;
            this.DateTime = datetime;
        }
        public int Value {get;}
        public DateTime DateTime { get; }
       
    }
}