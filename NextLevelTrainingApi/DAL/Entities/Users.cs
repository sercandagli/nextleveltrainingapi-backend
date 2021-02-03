using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Users")]
    public class Users : IDocument
    {
        public Users()
        {
            Teams = new List<Team>();
            UpcomingMatches = new List<UpcomingMatch>();
            Experiences = new List<Experience>();
            TrainingLocations = new List<TrainingLocation>();
            Coaches = new List<Coach>();
            Availabilities = new List<Availability>();
            TravelPostCodes = new List<TravelPostCode>();
            Reviews = new List<Review>();
            Qualifications = new List<UserQualification>();
            HiddenPosts = new List<HiddenPosts>();
            ConnectedUsers = new List<ConnectedUsers>();
            Credits = 0;
        }

        [BsonId]
        public Guid Id { get; set; }

        public string FullName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }

        public DateTime RegisterDate { get; set; }

        public string EmailID { get; set; }
        public string DeviceID { get; set; }
        public string DeviceToken { get; set; }

        public string MobileNo { get; set; }
        public string PostCode { get; set; }

        public string Password { get; set; }
        public string AccessToken { get; set; }

        public string PaypalPaymentId { get; set; }

        public string SocialLoginType { get; set; }
        public int? ProfileImageHeight { get; set; }
        public int? ProfileImageWidth { get; set; }
        public bool IsTempPassword { get; set; }
        public string Role { get; set; }
        public string DeviceType { get; set; }
        public bool Featured { get; set; }
        public string ProfileImage { get; set; }
        public string Achievements { get; set; }
        public string AboutUs { get; set; }
        public string Accomplishment { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public int Rate { get; set; }

        public int Credits { get; set; }

        public TravelMiles TravelMile { get; set; }
        public BankAccount BankAccount { get; set; }
        public List<Experience> Experiences { get; set; }
        public List<TravelPostCode> TravelPostCodes { get; set; }
        public List<Availability> Availabilities { get; set; }
        public DocumentDetail DBSCeritificate { get; set; }
        public DocumentDetail VerificationDocument { get; set; }
        public List<TrainingLocation> TrainingLocations { get; set; }
        public List<Team> Teams { get; set; }
        public List<UpcomingMatch> UpcomingMatches { get; set; }
        public List<Coach> Coaches { get; set; }
        public List<Review> Reviews { get; set; }
        public List<HiddenPosts> HiddenPosts { get; set; }
        public List<ConnectedUsers> ConnectedUsers { get; set; }

        public List<UserQualification> Qualifications { get; set; }
    }
}
