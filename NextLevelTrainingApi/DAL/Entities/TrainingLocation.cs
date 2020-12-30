using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class TrainingLocation
    {
        public Guid Id { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public Guid PlayerOrCoachID { get; set; }
    }
}
