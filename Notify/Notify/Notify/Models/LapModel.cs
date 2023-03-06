using System.Collections.Generic;

namespace Notify.Models
{
    public class LapModel
    {
        public int Number { get; set; }
        public List<TimingsModel> Timings { get; set; }
    }
}
