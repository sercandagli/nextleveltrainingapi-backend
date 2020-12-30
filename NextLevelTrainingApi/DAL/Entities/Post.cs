using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Posts")]
    public class Post:IDocument
    {

        public Post()
        {
            this.Comments = new List<Comment>();
            this.Likes = new List<Likes>();
        }

        [BsonId]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }

        public string MediaURL { get; set; }
        public int NumberOfLikes { get; set; }
        public bool IsVerified { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Comment> Comments { get; set; }
        public List<Likes> Likes { get; set; }
    }
}
