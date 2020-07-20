using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class GoogleUserViewModel
    {
       
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }

        public string Picture { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string AuthenticationToken { get; set; }
    }
}
