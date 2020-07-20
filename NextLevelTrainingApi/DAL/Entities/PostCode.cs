using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("PostCodes")]
    public class PostCode : IDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Code { get; set; }
    }
}
