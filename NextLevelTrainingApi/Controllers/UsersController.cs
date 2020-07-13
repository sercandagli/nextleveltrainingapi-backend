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

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("GetUser/{id}")]
        public ActionResult<Users> GetUser(Guid id)
        {
            var user = _unitOfWork.UserRepository.FindById(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        [Route("Register")]
        public ActionResult<Users> Register(UserViewModel userVM)
        {

            Users user = new Users()
            {
                Id = Guid.NewGuid(),
                Address = userVM.Address,
                EmailID = userVM.EmailID,
                FullName = userVM.FullName,
                MobileNo = userVM.MobileNo,
                Role = userVM.Role,
                Password = userVM.Password.Encrypt()
            };

            _unitOfWork.UserRepository.InsertOne(user);

            return user;
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult<Users> Login(UserViewModel userVM)
        {

            var user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == userVM.EmailID.ToLower() && x.Password == userVM.Password.Encrypt());

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
                Body = postVM.Body,
                CreatedDate = DateTime.Now,
                Header = postVM.Header,
                MediaURL = postVM.MediaURL,
                NumberOfLikes = postVM.NumberOfLikes
            };
            var user = _unitOfWork.UserRepository.FindById(postVM.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.Posts.Add(post);
            _unitOfWork.UserRepository.ReplaceOne(user);          

            return post;

        }

        [HttpGet]
        [Route("GetPostsByUser/{id}")]
        public ActionResult<List<Post>> GetPostsByUser(Guid id)
        {
            
            var user = _unitOfWork.UserRepository.FindById(id);

            if (user == null)
            {
                return NotFound();
            }

            return user.Posts;

        }

     
        [HttpPost]
        [Route("ChangeAboutUs")]
        public ActionResult<string> ChangeAboutUs(AboutViewModel aboutUsVM)
        {
            var user = _unitOfWork.UserRepository.FindById(aboutUsVM.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.AboutUs = aboutUsVM.AboutUs;
            _unitOfWork.UserRepository.ReplaceOne(user); 
            return aboutUsVM.AboutUs;

        }

        [HttpGet]
        [Route("GetAboutUs/{userId}")]
        public ActionResult<string> GetAboutUs(Guid userId)
        {

            var user = _unitOfWork.UserRepository.FindById(userId);

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
            var user = _unitOfWork.UserRepository.FindById(achievementVM.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.Achievements = achievementVM.Achievements;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return achievementVM.Achievements;

        }

        [HttpGet]
        [Route("GetAchievement/{userId}")]
        public ActionResult<string> GetAchievement(Guid userId)
        {

            var user = _unitOfWork.UserRepository.FindById(userId);

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
           var user = _unitOfWork.UserRepository.FindById(teamVM.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (teamVM.TeamID == null && teamVM.TeamID == Guid.Empty)
            {
                team = new Team()
                {
                    Id = Guid.NewGuid(),
                    TeamName = teamVM.TeamName,
                    TeamImage = teamVM.TeamImage,
                    StartDate = teamVM.StartDate,
                    EndDate = teamVM.EndDate,
                };
                user.Teams.Add(team);
               
            }
            else
            {
                team = user.Teams.Find(x => x.Id == teamVM.TeamID);
                if(team == null)
                {
                    return NotFound();
                }

                team.TeamImage = teamVM.TeamImage;
                team.TeamName = teamVM.TeamImage;
                team.StartDate = teamVM.StartDate;
                team.EndDate = teamVM.EndDate;


            }
            _unitOfWork.UserRepository.ReplaceOne(user);
            return team;

        }


        [HttpGet]
        [Route("GetTeams/{userId}")]
        public ActionResult<List<Team>> GetTeams(Guid userId)
        {

            var user = _unitOfWork.UserRepository.FindById(userId);

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
            var user = _unitOfWork.UserRepository.FindById(upcomingMatchVM.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (upcomingMatchVM.UpcomingMatchID == null && upcomingMatchVM.UpcomingMatchID == Guid.Empty)
            {
                upcomingMatch = new UpcomingMatch()
                {
                    Id = Guid.NewGuid(),
                    TeamName = upcomingMatchVM.TeamName,
                    MatchDate = upcomingMatchVM.MatchDate,
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
            }
           
            _unitOfWork.UserRepository.ReplaceOne(user);

            return upcomingMatch;

        }

        [HttpGet]
        [Route("GetUpcomingMatches/{userId}")]
        public ActionResult<List<UpcomingMatch>> GetUpcomingMatches(Guid userId)
        {

            var user = _unitOfWork.UserRepository.FindById(userId);

            if (user == null)
            {
                return NotFound();
            }

            return user.UpcomingMatches;

        }

    }
}
