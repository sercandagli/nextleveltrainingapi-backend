using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class PlayerCoachViewModel
    {
        public Guid PlayerId  { get; set; }
        public Guid CoachId { get; set; }
        //public string Status { get; set; }
    }
}
