using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingDataViewModel
    {

        public BookingDataViewModel()
        {
            Sessions = new List<BookinSessionViewModel>();
        }

        public Guid PlayerID { get; set; }
        public int BookingNumber { get; set; }
        public Guid CoachID { get; set; }
        public List<BookinSessionViewModel> Sessions { get; set; }
       
        public Guid TrainingLocationID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? RescheduledDateTime { get; set; }
        public DateTime? CancelledDateTime { get; set; }
    }


}
