using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class Experience
    {
        public Guid Id { get; set; }
        public string JobPosition { get; set; }
        public string Club { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyWorking { get; set; }
    }
}
