using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace NextLevelTrainingApi.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [JsonProperty("Name")]
        public string FullName { get; set; }

        [BsonElement("Adress")]
        [JsonProperty("Adress")]
        public string Address { get; set; }

        public string EmailID { get; set; }

        public string MobileNo { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
    }
}
