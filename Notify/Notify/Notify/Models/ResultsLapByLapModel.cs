using System;
using System.Collections.Generic;

namespace Notify.Models
{
    public class RaceResultsLapByLapModel
    {
        public int Season { get; set; }
        public int Round { get; set; }
        public string RaceName { get; set; }
        public DriverImageModel DriverImage { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public CircuitModel Circuit { get; set; }
        public List<LapModel> Laps { get; set; }
    }
}
