using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class AchievementViewModel
    {
        [Required]
        public Guid UserID { get; set; }
        public string Achievements { get; set; }
    }
}
