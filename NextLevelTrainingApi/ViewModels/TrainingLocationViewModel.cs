using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class TrainingLocationViewModel
    {
        
        public Guid? TrainingLocationId { get; set; }
        [Required]
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
        public Guid PlayerOrCoachID { get; set; }
    }
}
