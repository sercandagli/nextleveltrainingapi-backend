using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Bookings")]
    public class Booking : IDocument
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid PlayerID { get; set; }
        public int BookingNumber { get; set; }
        public Guid CoachID { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public DateTime SentDate { get; set; }

        public Guid TrainingLocationID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? RescheduledDateTime { get; set; }
        public DateTime? CancelledDateTime { get; set; }
    }
}
