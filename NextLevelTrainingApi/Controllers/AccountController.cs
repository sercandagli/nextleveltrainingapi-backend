using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using NextLevelTrainingApi.Models;
using NextLevelTrainingApi.ViewModels;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private readonly JWTAppSettings _jwtAppSettings;
        public AccountController(IUnitOfWork unitOfWork, IOptions<JWTAppSettings> jwtAppSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtAppSettings = jwtAppSettings.Value;
        }
        [HttpPost]
        [Route("Register")]
        public ActionResult<Users> Register(UserViewModel userVM)
        {
            Users user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == userVM.EmailID.ToLower());
            if (user != null)
            {
                return BadRequest("EmailID already registered");
            }

            user = new Users()
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
        public ActionResult<string> Login(LoginViewModel userVM)
        {

            var user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == userVM.EmailID.ToLower() && x.Password == userVM.Password.Encrypt());

            if (user == null)
            {
                return NotFound();
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtAppSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.EmailID),
                    new Claim("UserID", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;

        }
    }
}