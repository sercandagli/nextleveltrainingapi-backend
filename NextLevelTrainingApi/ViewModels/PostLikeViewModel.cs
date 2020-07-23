using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class PostLikeViewModel
    {
        public Guid PostID { get; set; }
        public int NumberOfLikes { get; set; }
    }

    public class PostLike
    {
        [Required]
        public Guid PostID { get; set; }
        [Required]
        public Guid UserID { get; set; }
    }
}
