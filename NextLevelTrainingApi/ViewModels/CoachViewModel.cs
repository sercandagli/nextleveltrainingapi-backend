using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class CoachViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string Achievements { get; set; }
        public string ProfileImage { get; set; }
        public string Accomplishment { get; set; }
        public string Status { get; set; }
        public string AboutUs { get; set; }
        public int Rate { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string AverageRating { get; set; }
        public List<Experience> Experiences { get; set; }
        public DocumentDetail DBSCeritificate { get; set; }
        public DocumentDetail VerificationDocument { get; set; }
        public List<Review> Reviews { get; set; }
        public List<UserQualification> Qualifications { get; set; }
        public List<Availability> Availabilities { get; set; }
        public List<Post> Posts { get; set; }
    }
}
