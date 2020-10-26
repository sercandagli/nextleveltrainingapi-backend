using System;
namespace NextLevelTrainingApi.DAL.Entities
{
    public class BookingTime
    {
        public BookingTime()
        {

        }

        public string Status { get; set; }

        public DateTime FromTime { get; set; }

        public DateTime ToTime { get; set; }

        public DateTime BookingDate { get; set; }
    }
}
