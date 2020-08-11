using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class SocialMediaLoginViewModel
    {
        [Required]
        public string Role { get; set; }
        [Required]
        public string DeviceID { get; set; }

        [Required]
        public string AuthenticationToken { get; set; }
        [Required]
        public string PostCode { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
