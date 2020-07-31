using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class PostViewModel
    {

        public string Header { get; set; }

        [Required]
        public string Body { get; set; }

        public string MediaURL { get; set; }
        public int NumberOfLikes { get; set; }
        public bool IsVerified { get; set; }

    }
}
