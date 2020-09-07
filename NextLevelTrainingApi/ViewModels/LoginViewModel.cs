using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string EmailID { get; set; }

        [Required]
        public string Password { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
    }
}
