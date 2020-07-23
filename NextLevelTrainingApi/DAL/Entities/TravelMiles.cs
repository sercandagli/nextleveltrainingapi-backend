using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class TravelMiles
    {
        public Guid CoachId { get; set; }
        public int TravelDistance { get; set; }
    }
}
