using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class UpcomingMatch
    {
        public Guid Id { get; set; }
        public string TeamName { get; set; }
        public DateTime MatchDate { get; set; }
    }
}
