using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class DocumentDetail
    {       
        public string Type { get; set; }
        public string Path { get; set; }
        public bool Verified { get; set; }
    }
}
