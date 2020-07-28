using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
        private EmailSettings _emailSettings;
        public AccountController(IUnitOfWork unitOfWork, IOptions<JWTAppSettings> jwtAppSettings, IOptions<EmailSettings> emailSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtAppSettings = jwtAppSettings.Value;
            _emailSettings = emailSettings.Value;
        }
        [HttpPost]
        [Route("Register")]
        public ActionResult<Users> Register(UserViewModel userVM)
        {
            Users user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == userVM.EmailID.ToLower());
            if (user != null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "EmailID already registered." } } });
            }

            user = new Users()
            {
                Id = Guid.NewGuid(),
                Address = userVM.Address,
                EmailID = userVM.EmailID,
                FullName = userVM.FullName,
                MobileNo = userVM.MobileNo,
                Role = userVM.Role,
                Password = userVM.Password.Encrypt(),
                Lat = userVM.Lat,
                Lng = userVM.Lng
            };

            _unitOfWork.UserRepository.InsertOne(user);

            if (user.Role.ToLower() == Constants.COACH)
            {
                EmailHelper.SendEmail(user.EmailID, _emailSettings, "signupcoach");
            }
            else
            {
                EmailHelper.SendEmail(user.EmailID, _emailSettings, "signupplayer");
            }

            return user;
        }

        [HttpGet]
        [Route("GetUserByEmail/{email}")]
        public ActionResult<Users> GetUserByEmail(string email)
        {

            return _unitOfWork.UserRepository.FilterBy(x => x.EmailID == email).SingleOrDefault();
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult<string> Login(LoginViewModel userVM)
        {

            var user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == userVM.EmailID.ToLower() && x.Password == userVM.Password.Encrypt());

            if (user == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Invalid credentials." } } });
            }

            string encryptedToken = GenerateToken(user);

            return encryptedToken;

        }

        private string GenerateToken(Users user)
        {
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

        [Route("FacebookLogin")]
        [HttpPost]
        public async Task<ActionResult<string>> FacebookLogin(SocialMediaLoginViewModel loginModel)
        {
            var result = await GetAsync<dynamic>(loginModel.AuthenticationToken, "https://graph.facebook.com/v2.8/", "me", "fields=first_name,last_name,email,picture.width(100).height(100)");
            if (result == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "No User found or invalid token." } } });
            }

            var fbUserVM = JsonConvert.DeserializeObject<FacebookUserViewModel>(result);

            if (string.IsNullOrEmpty(fbUserVM.Email))
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "No EmailID found." } } });
            }

            var user = _unitOfWork.UserRepository.FilterBy(x => x.EmailID == fbUserVM.Email).SingleOrDefault();
            if (user == null)
            {
                user = new Users();
                user.FullName = fbUserVM.FirstName + " " + fbUserVM.LastName;
                user.EmailID = fbUserVM.Email;
                user.Role = loginModel.Role;
                user.SocialLoginType = Constants.FACEBOOK_LOGIN;
                if (loginModel.Lat != null)
                {
                    user.Lat = loginModel.Lat;
                }
                if (loginModel.Lng != null)
                {
                    user.Lng = loginModel.Lng;
                }
                if (fbUserVM.Picture != null && fbUserVM.Picture.Data != null)
                {
                    user.ProfileImage = fbUserVM.Picture.Data.Url;
                    user.ProfileImageHeight = fbUserVM.Picture.Data.Height;
                    user.ProfileImageWidth = fbUserVM.Picture.Data.Width;
                }
                _unitOfWork.UserRepository.InsertOne(user);
            }
            else
            {
                if (user.Role.ToLower() != loginModel.Role.ToLower())
                {
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "EmailID already registered." } } });
                }
                user.FullName = fbUserVM.FirstName + " " + fbUserVM.LastName;
                user.EmailID = fbUserVM.Email;
                user.SocialLoginType = Constants.FACEBOOK_LOGIN;
                user.AccessToken = loginModel.AuthenticationToken;
                if (loginModel.Lat != null)
                {
                    user.Lat = loginModel.Lat;
                }
                if (loginModel.Lng != null)
                {
                    user.Lng = loginModel.Lng;
                }
                if (fbUserVM.Picture != null && fbUserVM.Picture.Data != null)
                {
                    user.ProfileImage = fbUserVM.Picture.Data.Url;
                    user.ProfileImageHeight = fbUserVM.Picture.Data.Height;
                    user.ProfileImageWidth = fbUserVM.Picture.Data.Width;
                }
                _unitOfWork.UserRepository.ReplaceOne(user);
            }

            string encryptedToken = GenerateToken(user);

            return encryptedToken;
        }

        private async Task<string> GetAsync<T>(string accessToken, string baseURL, string endpoint, string args = null)
        {
            HttpClient _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.GetAsync($"{endpoint}?access_token={accessToken}&{args}");
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }


        [Route("GoogleLogin")]
        [HttpPost]
        public ActionResult<string> GoogleLogin(GoogleUserViewModel loginModel)
        {

            var user = _unitOfWork.UserRepository.FilterBy(x => x.EmailID == loginModel.Email).SingleOrDefault();
            if (user == null)
            {
                user = new Users();
                user.FullName = loginModel.Name;
                user.EmailID = loginModel.Email;
                user.Role = loginModel.Role;
                user.SocialLoginType = Constants.GOOGLE_LOGIN;
                user.AccessToken = loginModel.AuthenticationToken;
                if (loginModel.Lat != null)
                {
                    user.Lat = loginModel.Lat;
                }
                if (loginModel.Lng != null)
                {
                    user.Lng = loginModel.Lng;
                }

                user.ProfileImage = loginModel.Picture;
                _unitOfWork.UserRepository.InsertOne(user);
            }
            else
            {
                if(user.Role.ToLower() != loginModel.Role.ToLower())
                {
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "EmailID already registered." } } });
                }
                user.FullName = loginModel.Name;
                user.AccessToken = loginModel.AuthenticationToken;
                if (loginModel.Lat != null)
                {
                    user.Lat = loginModel.Lat;
                }
                if (loginModel.Lng != null)
                {
                    user.Lng = loginModel.Lng;
                }

                user.ProfileImage = loginModel.Picture;
                _unitOfWork.UserRepository.ReplaceOne(user);
            }

            string encryptedToken = GenerateToken(user);

            return encryptedToken;
        }
    }
}