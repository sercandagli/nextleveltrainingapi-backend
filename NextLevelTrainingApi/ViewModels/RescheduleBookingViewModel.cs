using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class RescheduleBookingViewModel
    {
        public Guid BookingId { get; set; }
        public List<BookingTimeViewModel> Sessions{get;set;}
    }
}
