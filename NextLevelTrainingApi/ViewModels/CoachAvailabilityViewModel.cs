using System;

namespace NextLevelTrainingApi.ViewModels
{
    public class CoachAvailabilityViewModel
    {
        public Guid CoachID { get; set; }
        public DateTime date { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
