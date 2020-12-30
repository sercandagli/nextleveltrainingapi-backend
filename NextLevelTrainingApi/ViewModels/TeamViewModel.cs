using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class TeamViewModel
    {
        
        public Guid? TeamID { get; set; }

        [Required]
        public string TeamName { get; set; }

        public string TeamImage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
    }
}
