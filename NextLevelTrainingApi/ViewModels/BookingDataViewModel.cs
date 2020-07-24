using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingDataViewModel
    {
        public Guid PlayerID { get; set; }
        public int BookingNumber { get; set; }
        public Guid CoachID { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public DateTime BookingDate { get; set; }

        public Guid TrainingLocationID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? RescheduledDateTime { get; set; }
        public DateTime? CancelledDateTime { get; set; }
    }
}
