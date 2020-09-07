using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class UserDataViewModel
    {
        public UserDataViewModel()
        {
            this.Teams = new List<Team>();
            this.UpcomingMatches = new List<UpcomingMatch>();
            this.Experiences = new List<Experience>();
            this.TrainingLocations = new List<TrainingLocation>();
            this.Coaches = new List<Coach>();
            this.Availabilities = new List<AvailabilityViewModel>();
            this.TravelPostCodes = new List<TravelPostCode>();
            this.Reviews = new List<Review>();
            this.Qualifications = new List<UserQualification>();
            this.Posts = new List<PostDataViewModel>();
            this.Teams = new List<Team>();
            this.HiddenPosts = new List<HiddenPosts>();
            this.Bookings = new List<BookingViewModel>();
        }

        public Guid Id { get; set; }
        public string DeviceID { get; set; }
        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public bool IsTempPassword { get; set; }
        public string FullName { get; set; }

        public string PostCode { get; set; }

        public string Address { get; set; }

        public string EmailID { get; set; }

        public string MobileNo { get; set; }

        public string Password { get; set; }
        public string AccessToken { get; set; }

        public string SocialLoginType { get; set; }
        public int? ProfileImageHeight { get; set; }
        public int? ProfileImageWidth { get; set; }

        public string Role { get; set; }
        public string ProfileImage { get; set; }
        public string Achievements { get; set; }
        public string AboutUs { get; set; }
        public string Accomplishment { get; set; }
        public int Level { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public int Rate { get; set; }
        public string AverageBookingRating { get; set; }
        public TravelMiles TravelMile { get; set; }
        public BankAccount BankAccount { get; set; }
        public List<HiddenPosts> HiddenPosts { get; set; }
        public List<Experience> Experiences { get; set; }
        public List<TravelPostCode> TravelPostCodes { get; set; }
        public List<AvailabilityViewModel> Availabilities { get; set; }
        public DocumentDetail DBSCeritificate { get; set; }
        public DocumentDetail VerificationDocument { get; set; }
        public List<TrainingLocation> TrainingLocations { get; set; }
        public List<Team> Teams { get; set; }
        public List<UpcomingMatch> UpcomingMatches { get; set; }
        public List<Coach> Coaches { get; set; }
        public List<Review> Reviews { get; set; }

        public List<UserQualification> Qualifications { get; set; }
        public List<PostDataViewModel> Posts { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
    }
}
