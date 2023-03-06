namespace Notify.Models
{
    public class FastestLapModel
    {
        public int Rank { get; set; }
        public int Lap { get; set; }
        public TimeModel Time { get; set; }
        public SpeedModel AverageSpeed { get; set; }
    }
}
