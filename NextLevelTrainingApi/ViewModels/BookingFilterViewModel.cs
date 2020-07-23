using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingFilterViewModel
    {
        [Required]
        public Guid UserID { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
