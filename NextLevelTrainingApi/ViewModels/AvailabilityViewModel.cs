using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class AvailabilityViewModel
    {
        public string Day { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool IsWorking { get; set; }
    }
}
