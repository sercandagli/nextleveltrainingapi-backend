using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }

        public string MediaURL { get; set; }
        public int NumberOfLikes { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
