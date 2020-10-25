using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Notification")]
    public class Notification : IDocument
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid UserId { get; set; }
        public bool IsRead { get; set; }
        public string Image { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
