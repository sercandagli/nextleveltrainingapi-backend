using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingViewModel
    {
        public Guid Id { get; set; }
        public Guid PlayerID { get; set; }
        public int BookingNumber { get; set; }
        public Guid CoachID { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public DateTime SentDate { get; set; }
        public string FullName { get; set; }
        public TrainingLocation Location { get; set; }

        public Guid TrainingLocationID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? RescheduledDateTime { get; set; }
        public DateTime? CancelledDateTime { get; set; }
    }
}
