﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Users")]
    public class Users : IDocument
    {
        public Users()
        {
            this.Posts = new List<Post>();
        }
        [BsonId]

        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string EmailID { get; set; }

        public string MobileNo { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
        public string Achievements { get; set; }
        public string AboutUs { get; set; }
        public List<Post> Posts { get; set; }
        public List<Team> Teams { get; set; }
        public List<UpcomingMatch> UpcomingMatches { get; set; }
    }
}
