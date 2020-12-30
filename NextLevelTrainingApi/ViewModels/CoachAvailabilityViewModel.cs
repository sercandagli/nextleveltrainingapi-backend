using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class CoachAvailabilityViewModel
    {
        public CoachAvailabilityViewModel()
        {
            RequestedDates = new List<DateTime>();
        }

        public Guid CoachID { get; set; }
        public DateTime date { get; set; }

        public DateTime? EndDate { get; set; }


        public List<DateTime> RequestedDates { get; set; }
    }
}
