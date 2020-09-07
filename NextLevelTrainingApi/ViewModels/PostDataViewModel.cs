using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class PostDataViewModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string MediaURL { get; set; }
        public int NumberOfLikes { get; set; }
        public bool IsVerified { get; set; }

        public string CreatedBy { get; set; }
        public string ProfileImage { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<CommentedByViewModel> Comments { get; set; }
        public List<Likes> Likes { get; set; }

        public UserDataViewModel Poster { get; set; }
    }
}
