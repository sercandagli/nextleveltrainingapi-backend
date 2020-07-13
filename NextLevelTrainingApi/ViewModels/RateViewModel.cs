using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class RateViewModel
    {
        [Required]
        public Guid UserID { get; set; }
        public int Rate { get; set; }
    }
}
