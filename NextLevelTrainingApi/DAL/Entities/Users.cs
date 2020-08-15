using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Users")]
    public class Users : IDocument
    {
        public Users()
        {
            this.Teams = new List<Team>();
            this.UpcomingMatches = new List<UpcomingMatch>();
            this.Experiences = new List<Experience>();
            this.TrainingLocations = new List<TrainingLocation>();
            this.Coaches = new List<Coach>();
            this.Availabilities = new List<Availability>();
            this.TravelPostCodes = new List<TravelPostCode>();
            this.Reviews = new List<Review>();
            this.Qualifications = new List<UserQualification>();
            this.HiddenPosts = new List<HiddenPosts>();
            this.ConnectedUsers = new List<ConnectedUsers>();
        }
        [BsonId]

        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string EmailID { get; set; }
        public string DeviceID { get; set; }

        public string MobileNo { get; set; }
        public string PostCode { get; set; }

        public string Password { get; set; }
        public string AccessToken { get; set; }

        public string SocialLoginType { get; set; }
        public int? ProfileImageHeight { get; set; }
        public int? ProfileImageWidth { get; set; }
        public bool IsTempPassword { get; set; }
        public string Role { get; set; }
        public string ProfileImage { get; set; }
        public string Achievements { get; set; }
        public string AboutUs { get; set; }
        public string Accomplishment { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public int Rate { get; set; }
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
