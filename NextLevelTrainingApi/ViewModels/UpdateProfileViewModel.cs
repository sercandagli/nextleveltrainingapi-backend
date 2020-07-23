using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class UpdateProfileViewModel
    {
        public string FullName { get; set; }

        public string Address { get; set; }

        public string MobileNo { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
