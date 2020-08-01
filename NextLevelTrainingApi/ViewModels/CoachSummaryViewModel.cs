using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class CoachSummaryViewModel
    {
        public int BookingsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public List<SearchUserViewModel> Players { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}
