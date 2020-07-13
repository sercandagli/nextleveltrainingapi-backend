using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class TeamViewModel
    {
        public Guid UserID { get; set; }
        public Guid TeamID { get; set; }
        public string TeamName { get; set; }

        public string TeamImage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
    }
}
