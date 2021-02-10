using System;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("CreditHistory")]
    public class CreditHistory : IDocument
    {
        public CreditHistory()
        {

        }

        [BsonId]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Credits { get; set; }
        public int AmountPaid { get; set; }

        public Guid UserId { get; set; }

        public string PaypalPaymentId { get; set; }
    }
}
