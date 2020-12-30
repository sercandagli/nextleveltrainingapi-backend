using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("HashTags")]
    public class HashTag : IDocument
    {
        [BsonId]

        public Guid Id { get; set; }

        public string Tag { get; set; }
    }
}
