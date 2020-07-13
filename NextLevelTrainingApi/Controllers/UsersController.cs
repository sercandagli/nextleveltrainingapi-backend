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
            if (user == null)
            {
                return NotFound();
            }

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

    }
}
