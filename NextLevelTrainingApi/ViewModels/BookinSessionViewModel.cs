using System;
namespace NextLevelTrainingApi.ViewModels
{
    public class BookinSessionViewModel
    {
        public BookinSessionViewModel()
        {
        }

        public string FromTime { get; set; }

        public string ToTime { get; set; }

        public DateTime BookingDate { get; set; }
    }
}
