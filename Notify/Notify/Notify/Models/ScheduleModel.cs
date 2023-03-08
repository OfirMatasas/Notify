using System.Collections.Generic;

namespace Notify.Models
{
    public class ScheduleModel
    {
        public List<RaceEventModel> UpcomingRaceEvents { get; set; }
        public List<RaceEventModel> PastRaceEvents { get; set; }
    }
}
