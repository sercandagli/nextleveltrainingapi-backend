using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class CommentViewModel
    {
        public Guid PostId { get; set; }
        public string Text { get; set; }
        public Guid CommentedBy { get; set; }
    }
}
