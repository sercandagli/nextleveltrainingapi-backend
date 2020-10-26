using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingTimeViewModel
    {
        public BookingTimeViewModel()
        {
            Statuses = new List<BookingStatusViewModel>();
        }

        public string SessionStatus { get; set; }

        public List<BookingStatusViewModel> Statuses { get; set; }

        public DateTime FromTime { get; set; }

        public DateTime ToTime { get; set; }

        public DateTime BookingDate { get; set; }
    }
}
