using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class UserViewModel
    {
        [Required]
        public string FullName { get; set; }

        public string Address { get; set; }

        [Required]
        public string EmailID { get; set; }
        [Required]
        public string PostCode { get; set; }
        [Required]
        public string DeviceID { get; set; }
        public string DeviceToken { get; set; }

        public string MobileNo { get; set; }
        public string DeviceType { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public decimal? Lat { get; set; }

        public decimal? Lng { get; set; }
    }
}
