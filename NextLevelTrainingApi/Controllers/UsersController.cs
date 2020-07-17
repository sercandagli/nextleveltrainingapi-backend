using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextLevelTrainingApi.Models;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Repository;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.ViewModels;
using NextLevelTrainingApi.Helper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using NextLevelTrainingApi.AuthDetails;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private IUserContext _userContext;
        //private readonly HttpClient _httpClient;
        public UsersController(IUnitOfWork unitOfWork, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        [HttpGet]
        [Route("GetUser")]
        public ActionResult<Users> GetUser()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        [HttpPost]
        [Route("CreatePost")]
        public ActionResult<Post> CreatePost(PostViewModel postVM)
        {
            var post = new Post()
            {
                Id = Guid.NewGuid(),
                UserId = _userContext.UserID,
                Body = postVM.Body,
                CreatedDate = DateTime.Now,
                Header = postVM.Header,
                MediaURL = postVM.MediaURL,
                NumberOfLikes = postVM.NumberOfLikes
            };

            _unitOfWork.PostRepository.InsertOne(post);

            return post;

        }

        [HttpGet]
        [Route("GetPostsByUser")]
        public ActionResult<List<Post>> GetPostsByUser()
        {
            var posts = _unitOfWork.PostRepository.FilterBy(x=>x.UserId == _userContext.UserID).ToList();

            return posts;

        }


        [HttpPost]
        [Route("ChangeAboutUs")]
        public ActionResult<string> ChangeAboutUs(AboutViewModel aboutUsVM)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.AboutUs = aboutUsVM.AboutUs;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return aboutUsVM.AboutUs;

        }

        [HttpGet]
        [Route("GetAboutUs")]
        public ActionResult<string> GetAboutUs()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.AboutUs;

        }

        [HttpPost]
        [Route("ChangeAchievement")]
        public ActionResult<string> ChangeAchievement(AchievementViewModel achievementVM)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.Achievements = achievementVM.Achievements;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return achievementVM.Achievements;

        }

        [HttpGet]
        [Route("GetAchievement")]
        public ActionResult<string> GetAchievement()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.Achievements;

        }


        [HttpPost]
        [Route("SaveTeam")]
        public ActionResult<Team> SaveTeam(TeamViewModel teamVM)
        {
            var team = new Team();
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (teamVM.TeamID == null || teamVM.TeamID == Guid.Empty)
            {
                team = new Team()
                {
                    Id = Guid.NewGuid(),
                    TeamName = teamVM.TeamName,
                    TeamImage = teamVM.TeamImage,
                    StartDate = teamVM.StartDate,
                    EndDate = teamVM.EndDate
                };
                user.Teams.Add(team);

            }
            else
            {
                team = user.Teams.Find(x => x.Id == teamVM.TeamID);
                if (team == null)
                {
                    return NotFound();
                }

                team.TeamImage = teamVM.TeamImage;
                team.TeamName = teamVM.TeamImage;
                team.StartDate = teamVM.StartDate;
                team.EndDate = teamVM.EndDate;

                var toRemove = user.Teams.Find(x => x.Id == teamVM.TeamID);
                user.Teams.Remove(toRemove);
                user.Teams.Add(team);
            }
            _unitOfWork.UserRepository.ReplaceOne(user);
            return team;

        }


        [HttpGet]
        [Route("GetTeams")]
        public ActionResult<List<Team>> GetTeams()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.Teams;

        }

        [HttpPost]
        [Route("SaveUpcomingMatch")]
        public ActionResult<UpcomingMatch> SaveUpcomingMatch(UpcomingMatchViewModel upcomingMatchVM)
        {
            var upcomingMatch = new UpcomingMatch();
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (upcomingMatchVM.UpcomingMatchID == null || upcomingMatchVM.UpcomingMatchID == Guid.Empty)
            {
                upcomingMatch = new UpcomingMatch()
                {
                    Id = Guid.NewGuid(),
                    TeamName = upcomingMatchVM.TeamName,
                    MatchDate = upcomingMatchVM.MatchDate
                };
                user.UpcomingMatches.Add(upcomingMatch);
            }
            else
            {
                upcomingMatch = user.UpcomingMatches.Find(x => x.Id == upcomingMatchVM.UpcomingMatchID);
                if (upcomingMatch == null)
                {
                    return NotFound();
                }

                upcomingMatch.TeamName = upcomingMatchVM.TeamName;
                upcomingMatch.MatchDate = upcomingMatchVM.MatchDate;

                var toRemove = user.UpcomingMatches.Find(x => x.Id == upcomingMatchVM.UpcomingMatchID);
                user.UpcomingMatches.Remove(toRemove);
                user.UpcomingMatches.Add(upcomingMatch);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return upcomingMatch;

        }

        [HttpGet]
        [Route("GetUpcomingMatches")]
        public ActionResult<List<UpcomingMatch>> GetUpcomingMatches()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.UpcomingMatches;

        }

        [HttpPost]
        [Route("SaveExperience")]
        public ActionResult<Experience> SaveExperience(ExperienceViewModel experienceVM)
        {
            var experience = new Experience();
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (experienceVM.ExperienceId == null || experienceVM.ExperienceId == Guid.Empty)
            {
                experience = new Experience()
                {
                    Id = Guid.NewGuid(),
                    JobPosition = experienceVM.JobPosition,
                    Club = experienceVM.Club,
                    StartDate = experienceVM.StartDate,
                    EndDate = experienceVM.CurrentlyWorking ? DateTime.Now : experienceVM.EndDate
                };
                user.Experiences.Add(experience);
            }
            else
            {
                experience = user.Experiences.Find(x => x.Id == experienceVM.ExperienceId);
                if (experience == null)
                {
                    return NotFound();
                }

                experience.JobPosition = experienceVM.JobPosition;
                experience.Club = experienceVM.Club;
                experience.StartDate = experienceVM.StartDate;
                experience.EndDate = experienceVM.CurrentlyWorking ? DateTime.Now : experienceVM.EndDate;

                var toRemove = user.Experiences.Find(x => x.Id == experienceVM.ExperienceId);
                user.Experiences.Remove(toRemove);
                user.Experiences.Add(experience);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return experience;

        }

        [HttpGet]
        [Route("GetExperiences")]
        public ActionResult<List<Experience>> GetExperiences()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.Experiences;

        }

        [HttpPost]
        [Route("SaveDBSCeritificate")]
        public ActionResult<DocumentDetail> SaveDBSCeritificate(DocumentDetailViewModel documentDetailVM)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (user.DBSCeritificate == null)
            {
                user.DBSCeritificate = new DocumentDetail();
                user.DBSCeritificate.Path = documentDetailVM.Path;
                user.DBSCeritificate.Type = documentDetailVM.Type;
                user.DBSCeritificate.Verified = documentDetailVM.Verified;
            }
            else
            {
                user.DBSCeritificate.Path = documentDetailVM.Path;
                user.DBSCeritificate.Type = documentDetailVM.Type;
                user.DBSCeritificate.Verified = documentDetailVM.Verified;
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.DBSCeritificate;

        }

        [HttpGet]
        [Route("GetDBSCeritificate")]
        public ActionResult<DocumentDetail> GetDBSCeritificate()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.DBSCeritificate;

        }

        [HttpPost]
        [Route("SaveVerificationId")]
        public ActionResult<DocumentDetail> SaveVerificationId(DocumentDetailViewModel documentDetailVM)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (user.VerificationDocument == null)
            {
                user.VerificationDocument = new DocumentDetail();
                user.VerificationDocument.Path = documentDetailVM.Path;
                user.VerificationDocument.Type = documentDetailVM.Type;
                user.VerificationDocument.Verified = documentDetailVM.Verified;
            }
            else
            {
                user.VerificationDocument.Path = documentDetailVM.Path;
                user.VerificationDocument.Type = documentDetailVM.Type;
                user.VerificationDocument.Verified = documentDetailVM.Verified;
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.VerificationDocument;

        }

        [HttpGet]
        [Route("GetVerificationDocument")]
        public ActionResult<DocumentDetail> GetVerificationDocument()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.VerificationDocument;

        }

        [HttpPost]
        [Route("ChangeRate")]
        public ActionResult<int> ChangeRate(RateViewModel priceVM)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.Rate = priceVM.Rate;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return priceVM.Rate;

        }

        [HttpGet]
        [Route("GetRate")]
        public ActionResult<int> GetRate()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.Rate;

        }

        [HttpPost]
        [Route("SaveTrainingLocation")]
        public ActionResult<TrainingLocation> SaveTrainingLocation(TrainingLocationViewModel trainingLocationVM)
        {
            var trainingLocation = new TrainingLocation();
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            if (trainingLocationVM.TrainingLocationId == null || trainingLocationVM.TrainingLocationId == Guid.Empty)
            {
                trainingLocation = new TrainingLocation()
                {
                    Id = Guid.NewGuid(),
                    LocationName = trainingLocationVM.LocationName,
                    LocationAddress = trainingLocationVM.LocationAddress,
                    ImageUrl = trainingLocationVM.ImageUrl,
                    PlayerOrCoachID = trainingLocationVM.PlayerOrCoachID,
                    Role = trainingLocationVM.Role
                };
                user.TrainingLocations.Add(trainingLocation);
            }
            else
            {
                trainingLocation = user.TrainingLocations.Find(x => x.Id == trainingLocationVM.TrainingLocationId);
                if (trainingLocation == null)
                {
                    return NotFound();
                }

                trainingLocation.LocationName = trainingLocationVM.LocationName;
                trainingLocation.LocationAddress = trainingLocationVM.LocationAddress;
                trainingLocation.ImageUrl = trainingLocationVM.ImageUrl;
                trainingLocation.PlayerOrCoachID = trainingLocationVM.PlayerOrCoachID;
                trainingLocation.Role = trainingLocationVM.Role;

                var toRemove = user.TrainingLocations.Find(x => x.Id == trainingLocationVM.TrainingLocationId);
                user.TrainingLocations.Remove(toRemove);
                user.TrainingLocations.Add(trainingLocation);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return trainingLocation;

        }

        [HttpGet]
        [Route("GetTrainingLocations")]
        public ActionResult<List<TrainingLocation>> GetTrainingLocations()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            return user.TrainingLocations;

        }

        [HttpPost]
        [Route("UploadFile")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadFile([FromForm] FileInputModel file)
        {
            if (file == null || file.File.Length == 0)
                return Content("file not selected");

            string[] data = file.File.FileName.Split('.');
            string newFileName = data[0] + "-" + Guid.NewGuid().ToString() + "." + data[1];
            if (file.Type.ToLower() == "post")
            {
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Post",
                            newFileName);
                if (!System.IO.Directory.Exists(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Post")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Post"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }

                return "/Upload/Post/" + newFileName;
            }
            else if (file.Type.ToLower() == "location")
            {
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/TrainingLocation",
                            newFileName);
                if (!System.IO.Directory.Exists(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/TrainingLocation")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/TrainingLocation"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }

                return "/Upload/TrainingLocation/" + newFileName;
            }
            else
            {
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Profile",
                            newFileName);
                if (!System.IO.Directory.Exists(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Profile")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Profile"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {

                    await file.File.CopyToAsync(stream);
                }

                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

                if (user == null)
                {
                    return NotFound();
                }

                user.ProfileImage = "/Upload/Profile/" + newFileName; ;
                _unitOfWork.UserRepository.ReplaceOne(user);
                return user.ProfileImage;
            }
        }

        [HttpPost]
        [Route("SaveCoach")]
        public ActionResult<List<Coach>> SaveCoach(PlayerCoachViewModel playerCoachVM)
        {
            var coach = new Coach();
            var player = _unitOfWork.UserRepository.FilterBy(x => x.Id == playerCoachVM.PlayerId && x.Role == Constants.PLAYER).SingleOrDefault();
            if (player == null)
            {
                return NotFound();
            }

            coach = player.Coaches.Find(x => x.CoachId == playerCoachVM.CoachId && x.Status == playerCoachVM.Status);
            if (coach == null)
            {
                var c = new Coach()
                {
                    CoachId = playerCoachVM.CoachId,
                    Status = playerCoachVM.Status

                };
                player.Coaches.Add(c);
            }

            _unitOfWork.UserRepository.ReplaceOne(player);

            return player.Coaches;

        }

        [HttpGet]
        [Route("GetCoaches/{playerId}")]
        public ActionResult<List<CoachViewModel>> GetCoaches(Guid playerId)
        {

            var user = _unitOfWork.UserRepository.FilterBy(x => x.Id == playerId && x.Role.ToLower() == Constants.PLAYER).SingleOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            List<Guid> ids = user.Coaches.Select(x => x.CoachId).ToList();

            var coaches = _unitOfWork.UserRepository.FilterBy(x => ids.Contains(x.Id)).ToList().Select(x => new CoachViewModel
            {
                FullName = x.FullName,
                Address = x.Address,
                EmailID = x.EmailID,
                MobileNo = x.MobileNo,
                Achievements = x.Achievements,
                Experiences = x.Experiences
            }).ToList();

            return coaches;

        }

        [HttpGet]
        [Route("SaveBankAccount")]
        public ActionResult<BankAccount> SaveBankAccount(BankAccount bank)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.BankAccount = bank;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.BankAccount;

        }

        [HttpGet]
        [Route("GetBankAccount")]
        public ActionResult<BankAccount> GetBankAccount()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            return user.BankAccount;

        }

        [HttpPost]
        [Route("SaveAvailability")]
        public ActionResult<List<Availability>> SaveAvailability(Availability availability)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            var avail = user.Availabilities.Where(x => x.Day == availability.Day).SingleOrDefault();
            if (avail == null)
            {
                user.Availabilities.Add(availability);
            }
            else
            {
                user.Availabilities.Remove(avail);
                user.Availabilities.Add(availability);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.Availabilities;

        }

        [HttpPost]
        [Route("SaveAccomplishment")]
        public ActionResult<string> SaveAccomplishment(AccomplishmentViewModel accomplishment)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.Accomplishment = accomplishment.Accomplishment;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return accomplishment.Accomplishment;

        }

        [HttpPost]
        [Route("GetAccomplishment")]
        public ActionResult<string> GetAccomplishment()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            return user.Accomplishment;

        }

        [HttpPost]
        [Route("SaveTravelPostCode")]
        public ActionResult<TravelPostCode> SaveTravelPostCode(TravelPostCode postCode)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            var postC = user.TravelPostCodes.Where(x => x.PostCode == postCode.PostCode).SingleOrDefault();
            if (postC == null)
            {
                user.TravelPostCodes.Add(postCode);
            }

            return postCode;

        }

        [HttpPost]
        [Route("GetTravelPostCodes")]
        public ActionResult<List<TravelPostCode>> GetTravelPostCodes()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            return user.TravelPostCodes;

        }

        [HttpPost]
        [Route("SaveReview")]
        public ActionResult<ReviewViewModel> SaveReview(ReviewViewModel reviewVM)
        {            
            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == reviewVM.CoachId && x.Role == Constants.COACH).SingleOrDefault();
            if (coach == null)
            {
                return NotFound();
            }


            var review = new Review()
            {
                PlayerId = reviewVM.PlayerId,
                Rating = reviewVM.Rating,
                Feedback = reviewVM.Feedback
            };

            coach.Reviews.Add(review);
        
            _unitOfWork.UserRepository.ReplaceOne(coach);

            return reviewVM;

        }

        [HttpGet]
        [Route("GetReviews/{coachId}")]
        public ActionResult<List<Review>> GetReviews(Guid coachId)
        {

            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == coachId && x.Role.ToLower() == Constants.COACH).SingleOrDefault();
            if (coach == null)
            {
                return NotFound();
            }

            return coach.Reviews;

        }

        [HttpPost]
        [Route("SaveComment")]
        public ActionResult<Comment> SaveComment(CommentViewModel commentVM)
        {

            var Post = _unitOfWork.PostRepository.FilterBy(x => x.Id == commentVM.PostId).SingleOrDefault();
            if(Post == null)
            {
                return NotFound();
            }

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                CommentedBy = _userContext.UserID,
                Text = commentVM.Text,
                Commented = DateTime.Now
            };

            if(Post.Comments == null)
            {
                Post.Comments = new List<Comment>();
            }

            Post.Comments.Add(comment);

            _unitOfWork.PostRepository.ReplaceOne(Post);

            return comment;

        }

        [HttpGet]
        [Route("GetComments/{postId}")]
        public ActionResult<List<Comment>> GetComments(Guid postId)
        {
            var post = _unitOfWork.PostRepository.FindById(postId);
            if (post == null)
            {
                return NotFound();
            }            

            return post.Comments;

        }

        [HttpPost]
        [Route("SendMessage")]
        public ActionResult<MessageViewModel> SendMessage(MessageViewModel messageVM)
        {           

            var message = new Message()
            {
                Id = Guid.NewGuid(),
                Text = messageVM.Text,
                ImageUrl = messageVM.ImageUrl,
                ReceiverId = messageVM.ReceiverId,
                SenderId = messageVM.SenderId
            };

            _unitOfWork.MessageRepository.InsertOne(message);

            return messageVM;

        }
    }
}
