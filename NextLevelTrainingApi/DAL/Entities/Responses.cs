using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Leads")]
    public class Responses : IDocument
    {
        public Responses()
        {
            
        }

        [BsonId]
        public Guid Id { get; set; }

        public Guid CoachId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Leads Lead { get; set; }
    }
}
