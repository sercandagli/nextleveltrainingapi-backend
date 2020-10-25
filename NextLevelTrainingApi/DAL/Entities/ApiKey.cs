using System;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("ApiKeys")]
    public class ApiKey : IDocument
    {
        public ApiKey()
        {

        }

        [BsonId]
        public Guid Id { get; set; }

        public string Key { get; set; }
    }
}
