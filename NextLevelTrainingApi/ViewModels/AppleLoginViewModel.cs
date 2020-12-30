using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class AppleLoginViewModel
    {
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string DeviceID { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }

        public bool Featured { get; set; }

        [Required]
        public string Role { get; set; }
        public string PostCode { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
