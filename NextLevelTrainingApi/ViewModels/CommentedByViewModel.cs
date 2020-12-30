using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class CommentedByViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public Guid CommentedBy { get; set; }
        public DateTime Commented { get; set; }
    }
}
