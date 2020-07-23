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
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private IUserContext _userContext;
        private readonly JWTAppSettings _jwtAppSettings;
        //private readonly HttpClient _httpClient;
        public UsersController(IUnitOfWork unitOfWork, IUserContext userContext, IOptions<JWTAppSettings> jwtAppSettings)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _jwtAppSettings = jwtAppSettings.Value;
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
        [Route("UpdateProfile")]
        public ActionResult<Users> UpdateProfile(UpdateProfileViewModel profile)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = profile.FullName;
            user.Address = profile.Address;
            user.MobileNo = profile.MobileNo;
            user.Lat = profile.Lat;
            user.Lng = profile.Lng;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return user;
        }

        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult<bool> ChangePassword(ChangePasswordViewModel password)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return NotFound();
            }
            if (password.OldPassword.Encrypt() == user.Password)
            {
                user.Password = password.NewPassword.Encrypt();
                _unitOfWork.UserRepository.ReplaceOne(user);
            }
            else
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Old Password doesnot match." } } });
            }
            return true;
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
                //Header = postVM.Header,
                MediaURL = postVM.MediaURL,
                //NumberOfLikes = postVM.NumberOfLikes,
                IsVerified = true

            };

            _unitOfWork.PostRepository.InsertOne(post);

            return post;

        }

        [HttpPost]
        [Route("SaveLikeCountbyPost")]
        public ActionResult<Post> SaveLikeCountbyPost(PostLikeViewModel postVM)
        {
            var post = _unitOfWork.PostRepository.FindById(postVM.PostID);
            if (post == null)
            {
                return NotFound();
            }
            post.NumberOfLikes = postVM.NumberOfLikes;

            _unitOfWork.PostRepository.ReplaceOne(post);

            return post;

        }

        [HttpPost]
        [Route("SaveLikebyPost")]
        public ActionResult<Post> SaveLikebyPost(PostLike postVM)
        {
            var post = _unitOfWork.PostRepository.FindById(postVM.PostID);
            if (post == null)
            {
                return NotFound();
            }

            if (post.Likes.Count(x => x.UserId == postVM.UserID) == 0)
            {
                post.Likes.Add(new Likes() { UserId = postVM.UserID });
            }

            _unitOfWork.PostRepository.ReplaceOne(post);

            return post;

        }

        [HttpGet]
        [Route("GetLikesbyPost/{Id}")]
        public ActionResult<List<LikeViewModel>> GetLikesbyPost(Guid Id)
        {
            var post = _unitOfWork.PostRepository.FindById(Id);
            if (post == null)
            {
                return NotFound();
            }
            var likes = (from usr in _unitOfWork.UserRepository.AsQueryable()
                         join lke in post.Likes on usr.Id equals lke.UserId
                         select new LikeViewModel
                         {
                             FullName = usr.FullName,
                             ProfileImage = usr.ProfileImage,
                         }).ToList();

            return likes;

        }

        [HttpGet]
        [Route("GetPostsByUser/{coachId}")]
        public ActionResult<List<PostDataViewModel>> GetPostsByUser(Guid coachId)
        {
            string baseUrl = _jwtAppSettings.AppBaseURL;

            var userPosts = (from post in _unitOfWork.PostRepository.AsQueryable()
                                 //join usr in _unitOfWork.UserRepository.AsQueryable() on post.UserId equals usr.Id
                             where post.UserId == coachId
                             select new PostDataViewModel()
                             {
                                 Body = post.Body,
                                 CreatedDate = post.CreatedDate,
                                 Header = post.Header,
                                 Id = post.Id,
                                 IsVerified = post.IsVerified,
                                 Likes = post.Likes,
                                 MediaURL = baseUrl + post.MediaURL,
                                 NumberOfLikes = post.NumberOfLikes,
                                 UserId = post.UserId,
                                 //CreatedBy = usr.FullName,
                                 //ProfileImage = usr.ProfileImage,
                             }).ToList();

            foreach (var item in userPosts)
            {
                var puser = _unitOfWork.UserRepository.FindById(item.UserId);
                if (puser != null)
                {
                    item.ProfileImage = puser.ProfileImage != null ? ((puser.ProfileImage.Contains("http://") || puser.ProfileImage.Contains("https:/")) ? puser.ProfileImage : baseUrl + puser.ProfileImage) : "";
                    item.CreatedBy = puser.FullName;
                }
                var comments = _unitOfWork.PostRepository.FindById(item.Id).Comments;
                item.Comments = new List<CommentedByViewModel>();
                if (comments != null)
                {
                    foreach (var comment in comments)
                    {
                        var com = new CommentedByViewModel();
                        var user = _unitOfWork.UserRepository.FindById(comment.CommentedBy);
                        if (user != null)
                        {
                            com.ProfileImage = user.ProfileImage != null ? ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https:/")) ? user.ProfileImage : baseUrl + user.ProfileImage) : "";
                            com.FullName = user.FullName;
                        }
                        com.Id = comment.Id;
                        com.CommentedBy = comment.CommentedBy;
                        com.Commented = comment.Commented;
                        com.Text = comment.Text;
                        item.Comments.Add(com);
                    }
                }
            }

            return userPosts;
        }

        [HttpGet]
        [Route("GetAllPosts")]
        public ActionResult<List<PostDataViewModel>> GetAllPosts()
        {
            string baseUrl = _jwtAppSettings.AppBaseURL;

            var userPosts = (from post in _unitOfWork.PostRepository.AsQueryable()
                                 //join usr in _unitOfWork.UserRepository.AsQueryable() on post.UserId equals usr.Id
                             select new PostDataViewModel()
                             {
                                 Body = post.Body,
                                 CreatedDate = post.CreatedDate,
                                 Header = post.Header,
                                 Id = post.Id,
                                 IsVerified = post.IsVerified,
                                 Likes = post.Likes,
                                 MediaURL = baseUrl + post.MediaURL,
                                 NumberOfLikes = post.NumberOfLikes,
                                 UserId = post.UserId,
                                 //CreatedBy = usr.FullName,
                                 //ProfileImage = (usr.ProfileImage.Contains("http://") || usr.ProfileImage.Contains("https:/")) ? usr.ProfileImage : baseUrl + usr.ProfileImage
                             }).ToList();

            foreach (var item in userPosts)
            {
                var puser = _unitOfWork.UserRepository.FindById(item.UserId);
                if (puser != null)
                {
                    item.ProfileImage = puser.ProfileImage != null ? ((puser.ProfileImage.Contains("http://") || puser.ProfileImage.Contains("https:/")) ? puser.ProfileImage : baseUrl + puser.ProfileImage) : "";
                    item.CreatedBy = puser.FullName;
                }
                var comments = _unitOfWork.PostRepository.FindById(item.Id).Comments;
                item.Comments = new List<CommentedByViewModel>();
                if (comments != null)
                {
                    foreach (var comment in comments)
                    {
                        var com = new CommentedByViewModel();
                        var user = _unitOfWork.UserRepository.FindById(comment.CommentedBy);
                        if (user != null)
                        {
                            com.ProfileImage = user.ProfileImage != null ? ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https:/")) ? user.ProfileImage : baseUrl + user.ProfileImage) : "";
                            com.FullName = user.FullName;
                        }
                        com.Id = comment.Id;
                        com.CommentedBy = comment.CommentedBy;
                        com.Commented = comment.Commented;
                        com.Text = comment.Text;
                        item.Comments.Add(com);
                    }
                }
            }
            return userPosts;

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
                team.TeamName = teamVM.TeamName;
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
        [Route("DeleteTeam/{Id}")]
        public ActionResult<Team> DeleteTeam(Guid Id)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            var team = user.Teams.Find(x => x.Id == Id);
            if (team == null)
            {
                return NotFound();
            }

            user.Teams.Remove(team);
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
        [Route("DeleteUpcomingMatch/{Id}")]
        public ActionResult<UpcomingMatch> DeleteUpcomingMatch(Guid Id)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            var upcomingMatch = user.UpcomingMatches.Find(x => x.Id == Id);
            if (upcomingMatch == null)
            {
                return NotFound();
            }

            user.UpcomingMatches.Remove(upcomingMatch);

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
                    EndDate = experienceVM.EndDate,
                    CurrentlyWorking = experienceVM.CurrentlyWorking
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
                experience.EndDate = experienceVM.EndDate;
                experience.CurrentlyWorking = experienceVM.CurrentlyWorking;

                var toRemove = user.Experiences.Find(x => x.Id == experienceVM.ExperienceId);
                user.Experiences.Remove(toRemove);
                user.Experiences.Add(experience);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return experience;

        }

        [HttpGet]
        [Route("DeleteExperience/{Id}")]
        public ActionResult<Experience> DeleteExperience(Guid Id)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            var experience = user.Experiences.Find(x => x.Id == Id);
            if (experience == null)
            {
                return NotFound();
            }


            user.Experiences.Remove(experience);

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
            if (user.DBSCeritificate != null)
            {
                if (!string.IsNullOrEmpty(user.DBSCeritificate.Path))
                    user.DBSCeritificate.Path = _jwtAppSettings.AppBaseURL + user.DBSCeritificate.Path;
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

            if (user.VerificationDocument != null)
            {
                if (!string.IsNullOrEmpty(user.VerificationDocument.Path))
                    user.VerificationDocument.Path = _jwtAppSettings.AppBaseURL + user.VerificationDocument.Path;
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
                    Role = trainingLocationVM.Role,
                    Lat = trainingLocationVM.Lat,
                    Lng = trainingLocationVM.Lng
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
                trainingLocation.Lat = trainingLocationVM.Lat;
                trainingLocation.Lng = trainingLocationVM.Lng;

                var toRemove = user.TrainingLocations.Find(x => x.Id == trainingLocationVM.TrainingLocationId);
                user.TrainingLocations.Remove(toRemove);
                user.TrainingLocations.Add(trainingLocation);
            }

            _unitOfWork.UserRepository.ReplaceOne(user);

            return trainingLocation;

        }

        [HttpGet]
        [Route("DeleteTrainingLocation/{Id}")]
        public ActionResult<TrainingLocation> DeleteTrainingLocation(Guid Id)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }


            var trainingLocation = user.TrainingLocations.Find(x => x.Id == Id);
            if (trainingLocation == null)
            {
                return NotFound();
            }
            user.TrainingLocations.Remove(trainingLocation);

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

            foreach (var loc in user.TrainingLocations)
            {
                if (!string.IsNullOrEmpty(loc.ImageUrl))
                {
                    loc.ImageUrl = _jwtAppSettings.AppBaseURL + loc.ImageUrl;
                }
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
            string baseUrl = _jwtAppSettings.AppBaseURL;
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

                var post = _unitOfWork.PostRepository.FilterBy(x => x.Id == file.Id).SingleOrDefault();
                if (post != null)
                {
                    post.MediaURL = "/Upload/Post/" + newFileName;
                    _unitOfWork.PostRepository.ReplaceOne(post);
                }

                return baseUrl + "/Upload/Post/" + newFileName;
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

                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
                var loc = user.TrainingLocations.Where(x => x.Id == file.Id).SingleOrDefault();
                if (loc != null)
                {
                    loc.ImageUrl = "/Upload/TrainingLocation/" + newFileName;

                    var toremove = user.TrainingLocations.Where(x => x.Id == file.Id).SingleOrDefault();
                    user.TrainingLocations.Remove(toremove);
                    user.TrainingLocations.Add(loc);
                    _unitOfWork.UserRepository.ReplaceOne(user);
                }
                return baseUrl + "/Upload/TrainingLocation/" + newFileName;
            }
            else if (file.Type.ToLower() == "verification")
            {
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Verification",
                            newFileName);
                if (!System.IO.Directory.Exists(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Verification")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/Verification"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }

                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
                if (user.VerificationDocument != null)
                {
                    user.VerificationDocument.Path = "/Upload/Verification/" + newFileName;
                    _unitOfWork.UserRepository.ReplaceOne(user);
                }
                return baseUrl + "/Upload/Verification/" + newFileName;
            }
            else if (file.Type.ToLower() == "dbs")
            {
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/DBS",
                            newFileName);
                if (!System.IO.Directory.Exists(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/DBS")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/Upload/DBS"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }

                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
                if (user.DBSCeritificate != null)
                {
                    user.DBSCeritificate.Path = "/Upload/DBS/" + newFileName;
                    _unitOfWork.UserRepository.ReplaceOne(user);
                }
                return baseUrl + "/Upload/DBS/" + newFileName;
            }
            else if (file.Type.ToLower() == "profile")
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

                user.ProfileImage = "/Upload/Profile/" + newFileName;
                _unitOfWork.UserRepository.ReplaceOne(user);
                if (user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://"))
                {
                    return user.ProfileImage;
                }

                return baseUrl + user.ProfileImage;
            }
            return "";
        }

        [HttpPost]
        [Route("SaveCoach")]
        public ActionResult<bool> SaveCoach(PlayerCoachViewModel playerCoachVM)
        {
            var coach = new Coach();
            var player = _unitOfWork.UserRepository.FilterBy(x => x.Id == playerCoachVM.PlayerId).SingleOrDefault();
            if (player == null)
            {
                return NotFound();
            }

            coach = player.Coaches.Find(x => x.CoachId == playerCoachVM.CoachId);
            if (coach == null)
            {
                var c = new Coach()
                {
                    CoachId = playerCoachVM.CoachId,
                    Status = "Saved"

                };
                player.Coaches.Add(c);
            }
            else
            {
                player.Coaches.Remove(coach);
            }

            _unitOfWork.UserRepository.ReplaceOne(player);

            return true;

        }

        [HttpPost]
        [Route("GetCoaches")]
        public ActionResult<List<CoachViewModel>> GetCoaches(CoachFilterViewModel coach)
        {

            var user = _unitOfWork.UserRepository.FilterBy(x => x.Id == coach.PlayerId && x.Role.ToLower() == Constants.PLAYER).SingleOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            List<Coach> playerCoaches = user.Coaches.ToList();
            coach.Search = coach.Search.ToLower();
            var coaches = _unitOfWork.UserRepository.AsQueryable().Where(x => x.Role.ToLower() == Constants.COACH && ((coach.Search == null || coach.Search == "" || x.FullName.ToLower().Contains(coach.Search) || x.Address.ToLower().Contains(coach.Search) || x.EmailID.ToLower().Contains(coach.Search)))).ToList().Select(x => new CoachViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Address = x.Address,
                EmailID = x.EmailID,
                MobileNo = x.MobileNo,
                Achievements = x.Achievements,
                Accomplishment = x.Accomplishment,
                Experiences = x.Experiences,
                Availabilities = x.Availabilities,
                DBSCeritificate = x.DBSCeritificate,
                Qualifications = x.Qualifications,
                Reviews = x.Reviews,
                VerificationDocument = x.VerificationDocument,
                TrainingLocations = x.TrainingLocations,
                AboutUs = x.AboutUs,
                Rate = x.Rate,
                Lat = x.Lat,
                Lng = x.Lng,
                ProfileImage = x.ProfileImage,
                AverageRating = x.Reviews.Count() > 0 ? (x.Reviews.Select(x => x.Rating).Sum() / x.Reviews.Count()).ToString() : "New",
                Posts = _unitOfWork.PostRepository.FilterBy(z => z.UserId == x.Id).ToList(),
                Status = playerCoaches.Where(z => z.CoachId == x.Id).FirstOrDefault() == null ? "None" : playerCoaches.Where(z => z.CoachId == x.Id).First().Status
            }).ToList();

            foreach (var item in coaches)
            {
                item.ProfileImage = item.ProfileImage != null ? ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https:/")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage) : "";
                foreach (var p in item.Posts)
                {
                    p.MediaURL = p.MediaURL != null ? ((p.MediaURL.Contains("http://") || p.MediaURL.Contains("https:/")) ? p.MediaURL : _jwtAppSettings.AppBaseURL + p.MediaURL) : "";
                }
            }
            return coaches;

        }

        [HttpPost]
        [Route("DeleteCoach")]
        public ActionResult<List<Coach>> DeleteCoach(PlayerCoachViewModel playerCoachVM)
        {
            var player = _unitOfWork.UserRepository.FilterBy(x => x.Id == playerCoachVM.PlayerId && x.Role == Constants.PLAYER).SingleOrDefault();
            if (player == null)
            {
                return NotFound();
            }

            var coach = player.Coaches.Find(x => x.CoachId == playerCoachVM.CoachId);
            if (coach != null)
            {
                player.Coaches.Remove(coach);
                _unitOfWork.UserRepository.ReplaceOne(player);
            }


            return player.Coaches;

        }

        [HttpPost]
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
        public ActionResult<List<Availability>> SaveAvailability(List<Availability> availability)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.Availabilities = availability;

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.Availabilities;

        }


        [HttpGet]
        [Route("GetAvailability")]
        public ActionResult<List<Availability>> GetAvailability()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }


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
        public ActionResult<List<TravelPostCode>> SaveTravelPostCode(List<TravelPostCode> postCodes)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.TravelPostCodes = postCodes;

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.TravelPostCodes;

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

            var existReview = coach.Reviews.Find(x => x.Id == reviewVM.Id);
            if (existReview != null)
            {
                existReview.PlayerId = reviewVM.PlayerId;
                existReview.Rating = reviewVM.Rating;
                existReview.Feedback = reviewVM.Feedback;

                var toRemove = coach.Reviews.Find(x => x.Id == reviewVM.Id);
                coach.Reviews.Remove(toRemove);
                coach.Reviews.Add(existReview);

            }
            else
            {
                var review = new Review()
                {
                    PlayerId = reviewVM.PlayerId,
                    Rating = reviewVM.Rating,
                    Feedback = reviewVM.Feedback
                };
                coach.Reviews.Add(review);
            }


            _unitOfWork.UserRepository.ReplaceOne(coach);

            return reviewVM;

        }

        [HttpGet]
        [Route("DeleteReview/{Id}")]
        public ActionResult<Review> DeleteReview(Guid Id)
        {
            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == _userContext.UserID).SingleOrDefault();
            if (coach == null)
            {
                return NotFound();
            }

            var existReview = coach.Reviews.Find(x => x.Id == Id);
            if (existReview == null)
            {
                return NotFound();
            }

            coach.Reviews.Remove(existReview);

            _unitOfWork.UserRepository.ReplaceOne(coach);

            return existReview;

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
            if (Post == null)
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

            if (Post.Comments == null)
            {
                Post.Comments = new List<Comment>();
            }

            Post.Comments.Add(comment);

            _unitOfWork.PostRepository.ReplaceOne(Post);

            return comment;

        }

        [HttpGet]
        [Route("DeleteComment/{Id}")]
        public ActionResult<Comment> DeleteComment(Guid Id)
        {

            var Post = _unitOfWork.PostRepository.FilterBy(x => x.Comments.Where(z => z.Id == Id).Count() > 0).SingleOrDefault();
            if (Post == null)
            {
                return NotFound();
            }

            var comment = Post.Comments.Find(x => x.Id == Id);

            Post.Comments.Remove(comment);

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
                SenderId = messageVM.SenderId,
                SentDate = messageVM.SentDate
            };

            _unitOfWork.MessageRepository.InsertOne(message);

            return messageVM;

        }

        [HttpPost]
        [Route("GetMessagesBySenderAndReciever")]
        public ActionResult<List<Message>> GetMessagesBySenderAndReciever(SenderRecieverViewModel messageVM)
        {

            var messages = _unitOfWork.MessageRepository.FilterBy(x => x.SenderId == messageVM.SenderID && x.ReceiverId == messageVM.ReceiverId).ToList();

            return messages;

        }

        [HttpGet]
        [Route("GetLastMessages")]
        public ActionResult<List<LastMessageViewModel>> GetLastMessages()
        {
            List<Guid> senderIds = _unitOfWork.MessageRepository.FilterBy(x => x.ReceiverId == _userContext.UserID).OrderByDescending(x => x.SentDate).Select(x => x.SenderId).Distinct().ToList();
            List<Guid> receiverIds = _unitOfWork.MessageRepository.FilterBy(x => x.SenderId == _userContext.UserID).OrderByDescending(x => x.SentDate).Select(x => x.ReceiverId).Distinct().ToList();
            List<Guid> allIds = senderIds.Concat(receiverIds).Distinct().ToList();
            var messages = new List<LastMessageViewModel>();
            foreach (var id in allIds)
            {
                var message = (from msg in _unitOfWork.MessageRepository.AsQueryable()
                               join usr in _unitOfWork.UserRepository.AsQueryable() on msg.ReceiverId equals usr.Id
                               where (msg.SenderId == id && msg.ReceiverId == _userContext.UserID) || (msg.SenderId == _userContext.UserID && msg.ReceiverId == id)
                               select new LastMessageViewModel
                               {
                                   MessageID = msg.Id,
                                   Message = msg.Text,
                                   RecieverID = usr.Id,
                                   SenderID = msg.SenderId,
                                   ReceiverName = usr.FullName,
                                   ReceiverProfilePic = usr.ProfileImage,
                                   SentDate = msg.SentDate
                               }).OrderByDescending(x => x.SentDate).FirstOrDefault();
                if (message != null)
                    messages.Add(message);
            }


            return messages;

        }


        [HttpPost]
        [Route("SaveQualification")]
        public ActionResult<List<UserQualification>> SaveQualification(List<UserQualification> qualifications)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            user.Qualifications = new List<UserQualification>();
            if (qualifications != null)
            {
                foreach (var item in qualifications)
                {
                    item.Id = Guid.NewGuid();
                    user.Qualifications.Add(item);
                }
            }

            //var q = user.Qualifications.Where(x => x.Id == qualification.Id).SingleOrDefault();
            //if (q == null)
            //{
            //    qualification.Id = Guid.NewGuid();
            //    user.Qualifications.Add(qualification);
            //    _unitOfWork.UserRepository.ReplaceOne(user);
            //}
            //else
            //{
            //    var toremove = user.Qualifications.Where(x => x.Id == qualification.Id).SingleOrDefault();
            //    user.Qualifications.Remove(toremove);

            //    user.Qualifications.Add(q);
            //    _unitOfWork.UserRepository.ReplaceOne(user);
            //}

            _unitOfWork.UserRepository.ReplaceOne(user);
            return qualifications;

        }

        [HttpGet]
        [Route("GetQualifications")]
        public ActionResult<List<UserQualification>> GetQualifications()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);


            return user.Qualifications;

        }

        [HttpGet]
        [Route("GetPostCodes")]
        public ActionResult<List<string>> GetPostCodes()
        {
            //using (var reader = new StreamReader(@"C:\inetpub\wwwroot\NextLevelTrainingApi\postcodes.csv"))
            //{

            //    while (!reader.EndOfStream)
            //    {
            //        var line = reader.ReadLine();

            //        PostCode p = new PostCode();
            //        p.Id = new Guid();
            //        p.Code = line;
            //        _unitOfWork.PostCodeRepository.InsertOne(p);
            //    }
            //}

            var codes = _unitOfWork.PostCodeRepository.AsQueryable().Select(x => x.Code).ToList();
            return codes;
        }


        [HttpPost]
        [Route("SaveBooking")]
        public ActionResult<Booking> SaveBooking(Booking booking)
        {
            var user = _unitOfWork.UserRepository.FindById(booking.PlayerID);
            if (user == null)
            {
                return NotFound();
            }
            var coach = user.Coaches.Where(x => x.CoachId == booking.CoachID).SingleOrDefault();
            if (coach != null)
            {
                var toRemove = user.Coaches.Where(x => x.CoachId == booking.CoachID).SingleOrDefault();
                user.Coaches.Remove(toRemove);

                coach.Status = "Hired";
                user.Coaches.Add(coach);
                _unitOfWork.UserRepository.ReplaceOne(user);
            }
            booking.Id = Guid.NewGuid();
            if (_unitOfWork.BookingRepository.AsQueryable().Count() > 0)
            {
                booking.BookingNumber = _unitOfWork.BookingRepository.AsQueryable().Select(x => x.BookingNumber).Max() + 1;
            }
            else
            {
                booking.BookingNumber = 1;
            }
            _unitOfWork.BookingRepository.InsertOne(booking);

            return booking;

        }

        [HttpGet]
        [Route("CancelBooking/{BookingId}")]
        public ActionResult<Booking> CancelBooking(Guid bookingID)
        {

            var booking = _unitOfWork.BookingRepository.FindById(bookingID);
            booking.BookingStatus = "Cancelled";
            booking.CancelledDateTime = DateTime.Now;
            _unitOfWork.BookingRepository.ReplaceOne(booking);

            return booking;

        }

        [HttpPost]
        [Route("GetBookings")]
        public ActionResult<List<BookingViewModel>> GetBookings(BookingFilterViewModel booking)
        {
            List<BookingViewModel> bookings = new List<BookingViewModel>();

            if (booking.Role.ToLower() == Constants.COACH)
            {
                bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == booking.UserID).Select(x => new
                BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FromTime = x.FromTime,
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).SingleOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    ToTime = x.ToTime,
                    TransactionID = x.TransactionID
                }
                ).ToList();
            }
            else
            {
                bookings = _unitOfWork.BookingRepository.FilterBy(x => x.PlayerID == booking.UserID).Select(x => new
               BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FromTime = x.FromTime,
                    FullName = _unitOfWork.UserRepository.FindById(x.CoachID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).SingleOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    ToTime = x.ToTime,
                    TransactionID = x.TransactionID
                }
               ).ToList();
            }
            return bookings;
        }

        [HttpPost]
        [Route("RescheduleBooking")]
        public ActionResult<Booking> RescheduleBooking(RescheduleBookingViewModel booking)
        {

            var b = _unitOfWork.BookingRepository.FindById(booking.BookingId);
            b.FromTime = booking.FromTime;
            b.ToTime = booking.ToTime;
            b.BookingStatus = "Rescheduled";
            b.RescheduledDateTime = DateTime.Now;
            _unitOfWork.BookingRepository.ReplaceOne(b);

            return b;
        }


        [HttpPost]
        [Route("GetAvailableTimeByCoachId")]
        public ActionResult<List<string>> GetAvailableTimeByCoachId(CoachAvailabilityViewModel avalaibility)
        {
            var startDate = new DateTime(avalaibility.date.Year, avalaibility.date.Month, avalaibility.date.Day).AddTicks(1);
            var endDate = new DateTime(avalaibility.date.Year, avalaibility.date.Month, avalaibility.date.Day).AddDays(1).AddTicks(-2);
            var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == avalaibility.CoachID && (x.FromTime >= startDate && x.ToTime <= endDate)).ToList();
            var user = _unitOfWork.UserRepository.FindById(avalaibility.CoachID);
            if (user == null)
            {
                return NotFound();
            }
            var slots = user.Availabilities.Where(x => x.IsWorking == true).ToList();

            //List<string> bookedSlots = new List<string>();
            //foreach (var book in bookings)
            //{
            //    bookedSlots.Add(book.FromTime.ToString("hh:mm tt") + " - " + book.ToTime.ToString("hh:mm tt"));
            //}
            string day = avalaibility.date.DayOfWeek.ToString().ToLower();

            List<Availability> availableSlots = new List<Availability>();

            foreach (var slot in slots)
            {
                if (day == slot.Day.ToLower())
                {
                    DateTime start = slot.FromTime;
                    DateTime end = slot.FromTime;

                    for (int i = 0; i < 24; i++)
                    {
                        DateTime starttime = start.AddHours(i);
                        DateTime endtime = end.AddHours(i + 1);
                        if (endtime >= slot.ToTime)
                        {
                            break;
                        }
                        availableSlots.Add(new Availability() { FromTime = starttime, ToTime = endtime });

                        
                    }

                    break;
                }
            }

            List<string> freeSlots = new List<string>();

            foreach (var slot in availableSlots)
            {
                if (bookings.Where(x => slot.FromTime.TimeOfDay >= x.FromTime.TimeOfDay && slot.ToTime.TimeOfDay <= x.ToTime.TimeOfDay).Count() > 0)
                {

                }
                else
                {
                    freeSlots.Add(slot.FromTime.ToString("hh:mm tt") + "-" + slot.ToTime.ToString("hh:mm tt"));
                }
            }


            return freeSlots;

        }


        [HttpPost]
        [Route("SaveTravelMile")]
        public ActionResult<TravelMiles> SaveTravelMile(TravelMiles travel)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.TravelMile = travel;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return travel;

        }


        [HttpGet]
        [Route("GetTravelMile")]
        public ActionResult<TravelMiles> GetTravelMile()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return NotFound();
            }

            return user.TravelMile;
        }

    }
}
