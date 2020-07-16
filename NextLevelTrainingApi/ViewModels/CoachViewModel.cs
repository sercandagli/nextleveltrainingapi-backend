using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class CoachViewModel
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string Achievements { get; set; }
        public List<Experience> Experiences { get; set; }
    }
}
