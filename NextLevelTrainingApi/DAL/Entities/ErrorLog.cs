using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("ErrorLog")]
    public class ErrorLog: IDocument
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Exception { get; set; }
        public string StackTrace { get; set; }
    }
}
