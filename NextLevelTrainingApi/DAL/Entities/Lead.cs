using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Leads")]
    public class Leads : IDocument
    {
        public Leads()
        {
            CoachingType = new List<string>();
            Days = new List<string>();
            CoachingTime = new List<string>();
            DaysOfWeek = new List<string>();
            Web = false;
        }

        [BsonId]
        public Guid Id { get; set; }

        public string FullName { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Experience { get; set; }
        public string Age { get; set; }
        public string MaximumPrice { get; set; }
        public List<string> CoachingType { get; set; }
        public List<string> Days { get; set; }
        public List<string> CoachingTime { get; set; }
        public List<string> DaysOfWeek { get; set; }
        public bool Web { get; set; }

        public Guid? UserId { get; set; }
    }
}
