using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class DocumentDetailViewModel
    {
        public Guid UserId { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public bool Verified { get; set; }
    }
}
