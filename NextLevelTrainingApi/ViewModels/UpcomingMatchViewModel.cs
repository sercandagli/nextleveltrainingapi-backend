using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class UpcomingMatchViewModel
    {
        [Required]
        public Guid UserID { get; set; }
        public Guid? UpcomingMatchID { get; set; }

        [Required]
        public string TeamName { get; set; }
        public DateTime MatchDate { get; set; }
    }
}
