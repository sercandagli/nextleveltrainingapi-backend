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
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
using CorePush.Google;
using CorePush.Apple;
using Stripe;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private IUserContext _userContext;
        private readonly JWTAppSettings _jwtAppSettings;
        private readonly FCMSettings _fcmSettings;
        private readonly APNSettings _apnSettings;
        private EmailSettings _emailSettings;
        //private readonly HttpClient _httpClient;

        public UsersController(IUnitOfWork unitOfWork, IUserContext userContext, IOptions<JWTAppSettings> jwtAppSettings, IOptions<EmailSettings> emailSettings, IOptions<FCMSettings> fcmSettings, IOptions<APNSettings> apnSettings)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _jwtAppSettings = jwtAppSettings.Value;
            _emailSettings = emailSettings.Value;
            _fcmSettings = fcmSettings.Value;
            _apnSettings = apnSettings.Value;
        }

        [HttpGet]
        [Route("GetUser")]
        public  ActionResult<UserDataViewModel> GetUser()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
           
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            UserDataViewModel usr = new UserDataViewModel
            {
                Id = user.Id,
                DeviceType = user.DeviceType,
                FullName = user.FullName,
                DeviceID = user.DeviceID,
                State = user.State,
                Credits = user.Credits,
                DeviceToken = user.DeviceToken,
                EmailID = user.EmailID,
                AboutUs = user.AboutUs,
                AccessToken = user.AccessToken,
                Accomplishment = user.Accomplishment,
                IsTempPassword = user.IsTempPassword,
                Address = user.Address,
                Lat = user.Lat,
                Lng = user.Lng,
                Featured = user.Featured,
                PostCode = user.PostCode,
                Rate = user.Rate,
                ProfileImageHeight = user.ProfileImageHeight,
                ProfileImageWidth = user.ProfileImageWidth,
                SocialLoginType = user.SocialLoginType,
                Role = user.Role,
                MobileNo = user.MobileNo,
                ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage)
            };

            usr.Bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == usr.Id).Select(x => new BookingViewModel()
            {
                Amount = x.Amount,
                BookingNumber = x.BookingNumber,
                BookingStatus = x.BookingStatus,
                CoachID = x.CoachID,
                FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                Id = x.Id,
                Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                TrainingLocationID = x.TrainingLocationID,
                PaymentStatus = x.PaymentStatus,
                PlayerID = x.PlayerID,
                SentDate = x.SentDate,
                Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                {
                    BookingDate = x.BookingDate,
                    FromTime = x.FromTime,
                    ToTime = x.ToTime,
                    SessionStatus = x.Status
                }).ToList(),
                TransactionID = x.TransactionID,
                CancelledDateTime = x.CancelledDateTime,
                RescheduledDateTime = x.RescheduledDateTime,
                CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                {
                    BookingId = x.Id,
                    Feedback = b.Feedback,
                    Id = b.Id,
                    PlayerId = b.PlayerId,
                    Rating = b.Rating,
                    PlayerProfileImage = _unitOfWork.UserRepository.FindById(b.PlayerId).ProfileImage,
                    PlayerName = _unitOfWork.UserRepository.FindById(b.PlayerId).FullName,
                    CreatedDate = b.CreatedDate
                }).ToList()
            }).ToList();

            usr.Bookings.ForEach(b => b.BookingReviews.ForEach(r => r.PlayerProfileImage = string.IsNullOrEmpty(r.PlayerProfileImage) ? "" : ((r.PlayerProfileImage.Contains("http://") || r.PlayerProfileImage.Contains("https://")) ? r.PlayerProfileImage : _jwtAppSettings.AppBaseURL + r.PlayerProfileImage)));

            int total = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == usr.Id).SelectMany(r => r.Reviews).Sum(x => x.Rating);
            int count = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == usr.Id).SelectMany(r => r.Reviews).Count();
            usr.AverageBookingRating = count == 0 ? "New" : (total / count).ToString();

            List<AvailabilityViewModel> availaibilities = new List<AvailabilityViewModel>();
            foreach (var avail in user.Availabilities)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                availaibilities.Add(new AvailabilityViewModel()
                {
                    Day = avail.Day,
                    IsWorking = avail.IsWorking,
                    FromTime = avail.FromTime.ToString("hh: mm tt"),
                    ToTime = avail.ToTime.ToString("hh: mm tt"),
                });
            }

            usr.Availabilities = availaibilities;
            usr.BankAccount = user.BankAccount;
            usr.Achievements = user.Achievements;
            usr.Coaches = user.Coaches;
            usr.Experiences = user.Experiences;
            usr.DBSCeritificate = user.DBSCeritificate;
            if (usr.DBSCeritificate != null)
            {
                usr.DBSCeritificate.Path = string.IsNullOrEmpty(usr.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + usr.DBSCeritificate.Path;
            }
            usr.TrainingLocations = user.TrainingLocations;
            usr.TravelMile = user.TravelMile;
            usr.TravelPostCodes = user.TravelPostCodes;
            usr.Teams = user.Teams;
            usr.UpcomingMatches = user.UpcomingMatches;
            usr.VerificationDocument = user.VerificationDocument;
            if (usr.VerificationDocument != null)
            {
                usr.VerificationDocument.Path = string.IsNullOrEmpty(usr.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + usr.VerificationDocument.Path;
            }
            usr.Reviews = user.Reviews;
            usr.Qualifications = user.Qualifications;

            usr.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);

            List<Guid> hiddenPostIds = user.HiddenPosts.Select(x => x.PostId).ToList();
            var posts = _unitOfWork.PostRepository.FilterBy(x => x.UserId == usr.Id && !hiddenPostIds.Contains(x.Id)).Select(post => new PostDataViewModel()
            {
                Body = post.Body,
                CreatedDate = post.CreatedDate,
                Header = post.Header,
                Id = post.Id,
                IsVerified = post.IsVerified,
                Likes = post.Likes,
                MediaURL = post.MediaURL,
                NumberOfLikes = post.NumberOfLikes,
                UserId = post.UserId
            }).ToList();

            foreach (var p in posts)
            {
                var pUser = _unitOfWork.UserRepository.FindById(p.UserId);
                p.ProfileImage = pUser.ProfileImage;
                p.ProfileImage = string.IsNullOrEmpty(p.ProfileImage) ? "" : ((p.ProfileImage.Contains("http://") || p.ProfileImage.Contains("https://")) ? p.ProfileImage : _jwtAppSettings.AppBaseURL + p.ProfileImage);
                p.CreatedBy = pUser.FullName;

                try
                {
                    //string path = item.MediaURL.Replace(baseUrl + "/", "");
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + p.MediaURL);
                    System.Drawing.Image img = System.Drawing.Image.FromFile(fullPath);
                    p.Height = img.Height;
                    p.Width = img.Width;
                    p.MediaURL = _jwtAppSettings.AppBaseURL + p.MediaURL;
                }
                catch (Exception ex)
                {
                    p.MediaURL = _jwtAppSettings.AppBaseURL + p.MediaURL;
                }
            }

            usr.Posts = posts;
            return usr;
        }

        [HttpPost]
        [Route("SaveLead")]
        public async Task<ActionResult<Users>> SaveLead(SaveLeadViewModel input)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            var Location = user?.State ?? input.Location;

            var lead = _unitOfWork.LeadsRepository.FindOne(x => x.UserId == user.Id);

            if (lead != null)
            {
                lead.FullName = input.FullName;
                lead.EmailID = input.EmailID;
                lead.MobileNo = input.MobileNo;
                lead.Experience = input.Experience;
                lead.Age = input.Age;
                lead.CoachingType = input.CoachingType;
                lead.Days = input.Days;
                lead.CoachingTime = input.CoachingTime;
                lead.DaysOfWeek = input.DaysOfWeek;
                lead.Location = input.Location;

                _unitOfWork.LeadsRepository.ReplaceOne(lead);
            }
            else
            {
                lead = new Leads
                {
                    Id = user.Id,
                    FullName = input.FullName,
                    EmailID = input.EmailID,
                    MobileNo = input.MobileNo,
                    Experience = input.Experience,
                    Age = input.Age,
                    CoachingType = input.CoachingType,
                    Days = input.Days,
                    CoachingTime = input.CoachingTime,
                    DaysOfWeek = input.DaysOfWeek,
                    CreatedAt = DateTime.Now,
                    Location = input.Location,
                    UserId = user.Id
                };
                _unitOfWork.LeadsRepository.InsertOne(lead);
            }


            if (Location != null)
            {
                var county = Location.Split(", ").First().Split(" ").Last();
                var coaches = _unitOfWork.UserRepository.FilterBy(x => x.Role.ToLower() == Constants.COACH && x.State != null && x.State.Contains(county));

                foreach (var coach in coaches)
                {
                    await PushNotification(coach, $"{lead.FullName} is looking for Football Coaches in {Location}. Earn up to £{input.MaximumPrice ?? "20"} per hour", "New Lead");

                    try
                    {
                        var values = new Dictionary<string, string>
                        {
                            { "FullName", user.FullName },
                            { "Location", Location },
                            { "Phone", GetMaskedMobileNo(user.MobileNo) },
                            { "EmailID", GetMaskedEmail(user.EmailID) },
                            { "LatLng", $"{user.Lat},{user.Lng}" }
                        };
                        EmailHelper.SendEmail(coach.EmailID, _emailSettings, "newlead", values);
                    }
                    catch { }
                }
            }

            return user;
        }

        [HttpGet]
        [Route("GetLeads/{Location}")]
        public ActionResult<List<Leads>> GetLeads(string Location)
        {
            var leads = _unitOfWork.LeadsRepository.FilterBy(x => x.Location.ToLower().Contains(Location.ToLower()) && x.Web == true).ToList();

            return leads;
        }
        
        [HttpGet]
        [Route("GetLead/{Id}")]
        public ActionResult<LeadViewModel> GetLead(Guid Id)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var lead = _unitOfWork.LeadsRepository.FindOne(x => x.UserId == Id);

            if (lead == null)
            {
                return null;
            }

            var numResponses = _unitOfWork.ResponsesRepository.FilterBy(x => x.Lead.Id == lead.Id).Count();

            var leadVM = new LeadViewModel()
            {
                Id = lead.Id,
                FullName = lead.FullName,
                EmailID = lead.EmailID,
                MobileNo = lead.MobileNo,
                Experience = lead.Experience,
                Age = lead.Age,
                CoachingType = lead.CoachingType,
                Days = lead.Days,
                CoachingTime = lead.CoachingTime,
                DaysOfWeek = lead.DaysOfWeek,
                CreatedAt = lead.CreatedAt,
                Location = lead.Location,
                UserId = lead.UserId,
                NumResponses = numResponses
            };

            return leadVM;
        }

        [HttpPost]
        [Route("PurchaseLead")]
        public ActionResult<Responses> PurchaseLead(PurchaseLeadViewModel input)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var lead = _unitOfWork.LeadsRepository.FindById(input.LeadId);

            var exists = _unitOfWork.ResponsesRepository.FindOne(x => x.CoachId == user.Id && x.Lead.Id == lead.Id);

            if (exists != null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Lead already purchased." } } });
            }

            if (user.Credits == 0)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Not Enough Credits." } } });
            }

            var response = new Responses
            {
                Id = Guid.NewGuid(),
                CoachId = user.Id,
                CreatedAt = DateTime.Now,
                Lead = lead
            };

            user.Credits -= 1;

            _unitOfWork.UserRepository.ReplaceOne(user);
            _unitOfWork.ResponsesRepository.InsertOne(response);

            return response;
        }

        [HttpGet]
        [Route("GetResponses")]
        public ActionResult<List<Responses>> GetResponses()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var responses = _unitOfWork.ResponsesRepository.FilterBy(x => x.CoachId == user.Id).ToList();

            return responses;
        }

        [HttpPost]
        [Route("BuyCredits")]
        public ActionResult<CreditHistory> BuyCredits(BuyCreditsViewModel input)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var creditHistory = new CreditHistory()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Credits = input.Credits,
                AmountPaid = input.AmountPaid,
                UserId = user.Id,
                PaypalPaymentId = ""
            };
            _unitOfWork.CreditHistoryRepository.InsertOne(creditHistory);

            user.Credits += input.Credits;
            _unitOfWork.UserRepository.ReplaceOne(user);
            
            return creditHistory;
        }


        [HttpGet]
        [Route("GetCreditHistory")]
        public ActionResult<List<CreditHistory>> GetCreditHistory()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var creditHistory = _unitOfWork.CreditHistoryRepository.FilterBy(x => x.UserId == user.Id).ToList();

            return creditHistory;
        }

        [HttpPost]
        [Route("SendNotification")]
        public async Task<ActionResult<bool>> SendNotification(SendNotificationViewModel input)
        {
            var user = _unitOfWork.UserRepository.FindOne(x => x.EmailID.ToLower() == input.EmailID.ToLower());

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            await PushNotification(user, input.Message, input.Title ?? "Next Level");

            return true;
        }

        [HttpPost]
        [Route("SendMassNotification")]
        public async Task<ActionResult<bool>> SendMassNotification(SendNotificationViewModel input)
        {
            var users = _unitOfWork.UserRepository.FilterBy(x => x.DeviceToken != null);

            foreach (var user in users)
            {
                try
                {
                    await PushNotification(user, input.Message, input.Title ?? "Next Level");
                }
                catch { }
            }


            return true;
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<ActionResult<Users>> UpdateProfile(UpdateProfileViewModel profile)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            user.FullName = profile.FullName;
            user.Address = profile.Address;
            if (profile.State != null)
            {
                user.State = profile.State;
            }
            user.MobileNo = profile.MobileNo;
            user.Lat = profile.Lat;
            user.Lng = profile.Lng;
            _unitOfWork.UserRepository.ReplaceOne(user);

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = "Profile updated successfully.";
            notification.CreatedDate = DateTime.Now;
            notification.UserId = _userContext.UserID;
            _unitOfWork.NotificationRepository.InsertOne(notification);
            if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                await AndriodPushNotification(user.DeviceToken, notification);
            }
            else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                await ApplePushNotification(user.DeviceToken, notification);
            }
            return user;
        }

        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult<bool> ChangePassword(ChangePasswordViewModel password)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            if (password.OldPassword.Encrypt() == user.Password)
            {
                user.IsTempPassword = false;
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

            string[] tags = postVM.Body.Split(" ");

            foreach (string tag in tags)
            {
                if (!string.IsNullOrEmpty(tag) && tag.Trim().StartsWith("#"))
                {
                    HashTag hashTag = new HashTag();
                    hashTag.Id = Guid.NewGuid();
                    hashTag.Tag = tag;

                    var ifExists = _unitOfWork.HashTagRepository.FilterBy(x => x.Tag.ToLower() == tag.ToLower()).SingleOrDefault();
                    if (ifExists == null)
                    {
                        _unitOfWork.HashTagRepository.InsertOne(hashTag);
                    }
                }
            }

            if (postVM.TaggedUserIds.Count() > 0)
            {
                var users = _unitOfWork.UserRepository.FilterBy(x => postVM.TaggedUserIds.Contains(x.Id)).ToList();
                foreach (var user in users)
                {
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = user.FullName + " tagged you in a post.";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = user.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);

                    if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        AndriodPushNotification(user.DeviceToken, notification);
                    }
                    else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                         ApplePushNotification(user.DeviceToken, notification);
                    }
                }
            }

            return post;

        }

        [HttpPost]
        [Route("SaveLikeCountbyPost")]
        public ActionResult<Post> SaveLikeCountbyPost(PostLikeViewModel postVM)
        {
            var post = _unitOfWork.PostRepository.FindById(postVM.PostID);
            if (post == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }
            post.NumberOfLikes = postVM.NumberOfLikes;

            _unitOfWork.PostRepository.ReplaceOne(post);

            return post;

        }

        [HttpPost]
        [Route("SaveLikebyPost")]
        public async Task<ActionResult<Post>> SaveLikebyPost(PostLike postVM)
        {
            var post = _unitOfWork.PostRepository.FindById(postVM.PostID);
            if (post == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }

            var user = _unitOfWork.UserRepository.FindById(postVM.UserID);
            if(user == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });

            }

            if (post.Likes.Count(x => x.UserId == postVM.UserID) == 0)
            {
                post.Likes.Add(new Likes() { UserId = postVM.UserID });
            }

            _unitOfWork.PostRepository.ReplaceOne(post);

            var postOwner = _unitOfWork.UserRepository.FindById(post.UserId);

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = $"{user.FullName} liked your post";
            notification.CreatedDate = DateTime.Now;
            notification.UserId = postOwner.Id;
            notification.Image = user.ProfileImage;
            _unitOfWork.NotificationRepository.InsertOne(notification);
            if (postOwner.DeviceType != null && Convert.ToString(postOwner.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                await AndriodPushNotification(postOwner.DeviceToken, notification);
            }
            else if (postOwner.DeviceType != null && Convert.ToString(postOwner.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                await ApplePushNotification(postOwner.DeviceToken, notification);
            }

            return post;

        }

        [HttpGet]
        [Route("GetLikesbyPost/{Id}")]
        public ActionResult<List<LikeViewModel>> GetLikesbyPost(Guid Id)
        {
            var post = _unitOfWork.PostRepository.FindById(Id);
            if (post == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }
            List<Guid> ids = post.Likes.Select(x => x.UserId).ToList();
            //var likes = (from usr in _unitOfWork.UserRepository.us
            //             join post.li 
            //             select new LikeViewModel
            //             {
            //                 UserID = usr.Id,
            //                 FullName = usr.FullName,
            //                 ProfileImage = string.IsNullOrEmpty(usr.ProfileImage) ? "" : ((usr.ProfileImage.Contains("http://") || usr.ProfileImage.Contains("https:/")) ? usr.ProfileImage : _jwtAppSettings.AppBaseURL + usr.ProfileImage),
            //             }).ToList();

            List<LikeViewModel> likes = new List<LikeViewModel>();
            foreach (var user in post.Likes)
            {
                var like = (from usr in _unitOfWork.UserRepository.AsQueryable()
                            where usr.Id == user.UserId
                            select new LikeViewModel
                            {
                                UserId = usr.Id,
                                FullName = usr.FullName,
                                ProfileImage = usr.ProfileImage
                            }).SingleOrDefault();
                if (like != null)
                {
                    like.ProfileImage = string.IsNullOrEmpty(like.ProfileImage) ? "" : ((like.ProfileImage.Contains("http://") || like.ProfileImage.Contains("https:/")) ? like.ProfileImage : _jwtAppSettings.AppBaseURL + like.ProfileImage);
                    likes.Add(like);
                }
            }

            return likes;

        }

        [HttpGet]
        [Route("GetPostsByUser/{coachId}")]
        public ActionResult<List<PostDataViewModel>> GetPostsByUser(Guid coachId)
        {
            var coachUser = _unitOfWork.UserRepository.FindById(coachId);

            if (coachUser == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            string baseUrl = _jwtAppSettings.AppBaseURL;
            List<Guid> hiddenPostIds = coachUser.HiddenPosts.Select(x => x.PostId).ToList();
            var userPosts = (from post in _unitOfWork.PostRepository.AsQueryable()
                                 //join usr in _unitOfWork.UserRepository.AsQueryable() on post.UserId equals usr.Id
                             where post.UserId == coachId && !hiddenPostIds.Contains(post.Id)
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
            var usr = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (usr == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            List<Guid> hiddenPostIds = usr.HiddenPosts.Select(x => x.PostId).ToList();

            string baseUrl = _jwtAppSettings.AppBaseURL;

            var userPosts = (
                from post in _unitOfWork.PostRepository.AsQueryable()
                where !hiddenPostIds.Contains(post.Id)

                //join usr in _unitOfWork.UserRepository.AsQueryable() on post.UserId equals usr.Id
                select new PostDataViewModel()
                {
                    Body = post.Body,
                    CreatedDate = post.CreatedDate,
                    Header = post.Header,
                    Id = post.Id,
                    IsVerified = post.IsVerified,
                    Likes = post.Likes,
                    MediaURL = post.MediaURL,
                    NumberOfLikes = post.NumberOfLikes,
                    UserId = post.UserId,
                    //CreatedBy = usr.FullName,
                    //ProfileImage = (usr.ProfileImage.Contains("http://") || usr.ProfileImage.Contains("https:/")) ? usr.ProfileImage : baseUrl + usr.ProfileImage
                }
            ).ToList();

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

                item.Poster = _unitOfWork.UserRepository.FilterBy(x => x.Id == item.UserId).Select(x => new UserDataViewModel()
                {
                    Id = x.Id,
                    Role = x.Role,
                    Address = x.Address,
                    EmailID = x.EmailID,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    ProfileImage = x.ProfileImage,
                    DBSCeritificate = x.DBSCeritificate,
                    Teams = x.Teams,
                    Qualifications = x.Qualifications,
                    TrainingLocations = x.TrainingLocations,
                    Rate = x.Rate,
                    VerificationDocument = x.VerificationDocument,
                    PostCode = x.PostCode,
                    AboutUs = x.AboutUs,
                    Achievements = x.Achievements,
                    Accomplishment = x.Accomplishment,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    TravelMile = x.TravelMile,
                    HiddenPosts = x.HiddenPosts,
                    Experiences = x.Experiences

                }).SingleOrDefault();

                if (item.Poster != null)
                {
                    item.Poster.ProfileImage = string.IsNullOrEmpty(item.Poster.ProfileImage) ? "" : ((item.Poster.ProfileImage.Contains("http://") || item.Poster.ProfileImage.Contains("https://")) ? item.Poster.ProfileImage : _jwtAppSettings.AppBaseURL + item.Poster.ProfileImage);
                }

                if (!string.IsNullOrEmpty(item.MediaURL))
                {
                    try
                    {
                        //string path = item.MediaURL.Replace(baseUrl + "/", "");
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + item.MediaURL);
                        Image img = Image.FromFile(fullPath);
                        item.Height = img.Height;
                        item.Width = img.Width;
                        item.MediaURL = baseUrl + item.MediaURL;
                    }
                    catch
                    {
                        item.MediaURL = baseUrl + item.MediaURL;
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Team not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var team = user.Teams.Find(x => x.Id == Id);
            if (team == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Team not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Upcoming Match not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var upcomingMatch = user.UpcomingMatches.Find(x => x.Id == Id);
            if (upcomingMatch == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Upcoming Match not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Experience not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var experience = user.Experiences.Find(x => x.Id == Id);
            if (experience == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Experience not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            if (user.DBSCeritificate == null)
            {
                user.DBSCeritificate = new DocumentDetail();
                user.DBSCeritificate.Path = documentDetailVM.Path;
                user.DBSCeritificate.Type = documentDetailVM.Type;
                //user.DBSCeritificate.Verified = true;
            }
            else
            {
                user.DBSCeritificate.Path = documentDetailVM.Path;
                user.DBSCeritificate.Type = documentDetailVM.Type;
                //user.DBSCeritificate.Verified = documentDetailVM.Verified;
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            if (user.VerificationDocument == null)
            {
                user.VerificationDocument = new DocumentDetail();
                user.VerificationDocument.Path = documentDetailVM.Path;
                user.VerificationDocument.Type = documentDetailVM.Type;
                //user.VerificationDocument.Verified = true;
            }
            else
            {
                user.VerificationDocument.Path = documentDetailVM.Path;
                user.VerificationDocument.Type = documentDetailVM.Type;
                //user.VerificationDocument.Verified = documentDetailVM.Verified;
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                    return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Location not found." } } });
                }

                trainingLocation.LocationName = trainingLocationVM.LocationName;
                trainingLocation.LocationAddress = trainingLocationVM.LocationAddress;
                //trainingLocation.ImageUrl = trainingLocationVM.ImageUrl;
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }


            var trainingLocation = user.TrainingLocations.Find(x => x.Id == Id);
            if (trainingLocation == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Location not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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


        [Route("UpdateDeviceToken")]
        [HttpPost]
        public ActionResult UpdateDeviceToken(UpdateDeviceTokenRequest updateDeviceTokenRequest)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            user.DeviceToken = updateDeviceTokenRequest.DeviceToken;
            _unitOfWork.UserRepository.ReplaceOne(user);
            return Ok();
        }

        [HttpPost]
        [Route("UploadFile")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadFile([FromForm] FileInputModel file)
        {
            if (file == null || file.File.Length == 0)
                return Content("file not selected");

            String ret = Regex.Replace(file.File.FileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            string fName = ret.Replace(" ", String.Empty);
            string extension = Path.GetExtension(fName);
            string[] data = fName.Split(extension);
            string baseUrl = _jwtAppSettings.AppBaseURL;
            string newFileName = data[0] + "-" + Guid.NewGuid().ToString() + extension;
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

                if (file.File.ContentType.Contains("image/"))
                {
                    CompressImage(path, file.File);
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
                if (file.File.ContentType.Contains("image/"))
                {
                    CompressImage(path, file.File);
                }
                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
                var loc = user.TrainingLocations.Where(x => x.Id == file.Id).SingleOrDefault();
                if (loc != null)
                {
                    loc.ImageUrl = "/Upload/TrainingLocation/" + newFileName;

                    var toremove = user.TrainingLocations.Where(x => x.Id == file.Id).FirstOrDefault();
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
                if (file.File.ContentType.Contains("image/"))
                {
                    CompressImage(path, file.File);
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
                if (file.File.ContentType.Contains("image/"))
                {
                    CompressImage(path, file.File);
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

                if (file.File.ContentType.Contains("image/"))
                {
                    CompressImage(path, file.File);
                }
                var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

                if (user == null)
                {
                    return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "Player not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            List<Coach> playerCoaches = user.Coaches.ToList();
            coach.Search = Convert.ToString(coach.Search).ToLower();
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
                TravelMile = x.TravelMile,
                ProfileImage = x.ProfileImage,
                HiddenPosts = x.HiddenPosts,
                AverageRating = x.Reviews.Count() > 0 ? (x.Reviews.Select(x => x.Rating).Sum() / x.Reviews.Count()).ToString() : "New",
                Posts = _unitOfWork.PostRepository.FilterBy(z => z.UserId == x.Id).ToList(),
                Status = playerCoaches.Where(z => z.CoachId == x.Id).FirstOrDefault() == null ? "None" : playerCoaches.Where(z => z.CoachId == x.Id).First().Status,
                //Bookings = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == x.Id).ToList()
            }).ToList();

            foreach (var item in coaches)
            {
                item.Bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == item.Id).Select(x => new
                  BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    TransactionID = x.TransactionID,
                    CancelledDateTime = x.CancelledDateTime,
                    RescheduledDateTime = x.RescheduledDateTime,
                    CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                    Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                    {
                        BookingDate = x.BookingDate,
                        ToTime = x.ToTime,
                        FromTime = x.FromTime,
                        SessionStatus = x.Status
                    }).ToList(),
                    BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                    {
                        BookingId = x.Id,
                        Feedback = b.Feedback,
                        Id = b.Id,
                        PlayerId = b.PlayerId,
                        Rating = b.Rating,
                        PlayerProfileImage = _unitOfWork.UserRepository.FindById(b.PlayerId).ProfileImage,
                        PlayerName = _unitOfWork.UserRepository.FindById(b.PlayerId).FullName,
                        CreatedDate = b.CreatedDate
                    }).ToList()
                }
                ).ToList();

                item.Bookings.ForEach(b => b.BookingReviews.ForEach(x => x.PlayerProfileImage = x.PlayerProfileImage != null ? ((x.PlayerProfileImage.Contains("http://") || x.PlayerProfileImage.Contains("https:/")) ? x.PlayerProfileImage : _jwtAppSettings.AppBaseURL + x.PlayerProfileImage) : ""));
                item.ProfileImage = item.ProfileImage != null ? ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https:/")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage) : "";
                foreach (var p in item.Posts)
                {
                    p.MediaURL = p.MediaURL != null ? ((p.MediaURL.Contains("http://") || p.MediaURL.Contains("https:/")) ? p.MediaURL : _jwtAppSettings.AppBaseURL + p.MediaURL) : "";
                }
                List<Guid> ids = item.HiddenPosts.Select(x => x.PostId).ToList();
                item.Posts = item.Posts.Where(x => !ids.Contains(x.Id)).ToList();

                int total = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Sum(x => x.Rating);
                int count = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Count();
                item.AverageBookingRating = count == 0 ? "New" : (total / count).ToString();

                var coachBookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == item.Id);
                item.Level = coachBookings.Sum(x => x.Sessions.Where(x => x.Status == "completed").Count());
                item.Level = Convert.ToInt32(Math.Ceiling((double)item.Level / 50));
                if (item.Level == 0)
                {
                    item.Level = 1;
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "Player not found." } } });
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
        public ActionResult<DAL.Entities.BankAccount> SaveBankAccount(DAL.Entities.BankAccount bank)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            user.BankAccount = bank;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.BankAccount;

        }

        [HttpGet]
        [Route("GetBankAccount")]
        public ActionResult<DAL.Entities.BankAccount> GetBankAccount()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            return user.BankAccount;

        }

        [HttpPost]
        [Route("SaveAvailability")]
        public ActionResult<List<Availability>> SaveAvailability(List<AvailabilityViewModel> availability)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            List<Availability> availaibilities = new List<Availability>();
            foreach (var avail in availability)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                availaibilities.Add(new Availability()
                {
                    Day = avail.Day,
                    IsWorking = avail.IsWorking,
                    FromTime = DateTime.ParseExact(date + " " + avail.FromTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture),
                    ToTime = DateTime.ParseExact(date + " " + avail.ToTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture)
                });
            }

            user.Availabilities = availaibilities;

            _unitOfWork.UserRepository.ReplaceOne(user);

            return user.Availabilities;

        }


        [HttpGet]
        [Route("GetAvailability")]
        public ActionResult<List<AvailabilityViewModel>> GetAvailability()
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            List<AvailabilityViewModel> availaibilities = new List<AvailabilityViewModel>();
            foreach (var avail in user.Availabilities)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                availaibilities.Add(new AvailabilityViewModel()
                {
                    Day = avail.Day,
                    IsWorking = avail.IsWorking,
                    FromTime = avail.FromTime.ToString("hh: mm tt"),
                    ToTime = avail.ToTime.ToString("hh: mm tt"),
                });
            }
            return availaibilities;

        }

        [HttpPost]
        [Route("SaveAccomplishment")]
        public ActionResult<string> SaveAccomplishment(AccomplishmentViewModel accomplishment)
        {

            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            return user.TravelPostCodes;

        }

        [HttpPost]
        [Route("SaveReview")]
        public ActionResult<ReviewViewModel> SaveReview(ReviewViewModel reviewVM)
        {
            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == reviewVM.CoachId && x.Role.ToLower() == Constants.COACH).SingleOrDefault();
            if (coach == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var existReview = coach.Reviews.Find(x => x.Id == reviewVM.Id);
            if (existReview != null)
            {
                existReview.PlayerId = reviewVM.PlayerId;
                existReview.Rating = reviewVM.Rating;
                existReview.Feedback = reviewVM.Feedback;
                existReview.CreatedDate = DateTime.Now;

                var toRemove = coach.Reviews.Find(x => x.Id == reviewVM.Id);
                coach.Reviews.Remove(toRemove);
                coach.Reviews.Add(existReview);

            }
            else
            {
                var review = new DAL.Entities.Review()
                {
                    Id = Guid.NewGuid(),
                    PlayerId = reviewVM.PlayerId,
                    Rating = reviewVM.Rating,
                    Feedback = reviewVM.Feedback,
                    CreatedDate = DateTime.Now
                };
                coach.Reviews.Add(review);
            }


            _unitOfWork.UserRepository.ReplaceOne(coach);

            return reviewVM;

        }

        [HttpGet]
        [Route("DeleteReview/{Id}")]
        public ActionResult<DAL.Entities.Review> DeleteReview(Guid Id)
        {
            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == _userContext.UserID).SingleOrDefault();
            if (coach == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var existReview = coach.Reviews.Find(x => x.Id == Id);
            if (existReview == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Review not found." } } });
            }

            coach.Reviews.Remove(existReview);

            _unitOfWork.UserRepository.ReplaceOne(coach);

            return existReview;

        }

        [HttpGet]
        [Route("GetReviews/{coachId}")]
        public ActionResult<List<ReviewDataViewModel>> GetReviews(Guid coachId)
        {

            var coach = _unitOfWork.UserRepository.FilterBy(x => x.Id == coachId && x.Role.ToLower() == Constants.COACH).SingleOrDefault();
            if (coach == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            List<ReviewDataViewModel> reviews = new List<ReviewDataViewModel>();
            foreach (var ch in coach.Reviews)
            {
                ReviewDataViewModel review = new ReviewDataViewModel();
                review.Id = ch.Id;
                review.CoachId = coach.Id;
                review.PlayerId = ch.PlayerId;
                review.Rating = ch.Rating;
                review.Feedback = ch.Feedback;
                review.CreatedDate = ch.CreatedDate;
                var user = _unitOfWork.UserRepository.FindById(ch.PlayerId);
                review.PlayerProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage);
                review.PlayerName = user.FullName;
                reviews.Add(review);
            }
            return reviews;

        }

        [HttpPost]
        [Route("SaveComment")]
        public ActionResult<Comment> SaveComment(CommentViewModel commentVM)
        {

            var Post = _unitOfWork.PostRepository.FilterBy(x => x.Id == commentVM.PostId).SingleOrDefault();
            if (Post == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                CommentedBy = commentVM.CommentedBy,
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
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }

            var comment = Post.Comments.Find(x => x.Id == Id);

            Post.Comments.Remove(comment);

            _unitOfWork.PostRepository.ReplaceOne(Post);

            return comment;

        }


        [HttpGet]
        [Route("GetComments/{postId}")]
        public ActionResult<List<CommentedByViewModel>> GetComments(Guid postId)
        {
            var post = _unitOfWork.PostRepository.FindById(postId);
            if (post == null)
            {
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Post not found." } } });
            }

            List<Guid> ids = post.Comments.Select(x => x.CommentedBy).ToList();
            var users = _unitOfWork.UserRepository.FilterBy(x => ids.Contains(x.Id)).ToList();

            List<CommentedByViewModel> comments = new List<CommentedByViewModel>();
            foreach (var comment in post.Comments)
            {
                CommentedByViewModel cm = new CommentedByViewModel();
                cm.Commented = comment.Commented;
                cm.Id = comment.Id;
                cm.Text = comment.Text;
                cm.CommentedBy = comment.CommentedBy;
                var user = users.Where(x => x.Id == cm.CommentedBy).SingleOrDefault();
                if (user != null)
                {
                    cm.FullName = user.FullName;
                    cm.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage);
                }
                comments.Add(cm);
            }

            return comments;

        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<ActionResult<MessageViewModel>> SendMessage(MessageViewModel messageVM)
        {
            var noMessages = _unitOfWork.MessageRepository.FilterBy(x => x.SenderId == messageVM.SenderId && x.ReceiverId == messageVM.ReceiverId).Count();

            if (noMessages == 0)
            {
                var user = _unitOfWork.UserRepository.FindById(messageVM.SenderId);
                if (user.Credits > 0)
                {
                    user.Credits -= 1;
                    _unitOfWork.UserRepository.ReplaceOne(user);
                }
            }

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

            var usr = _unitOfWork.UserRepository.FindById(messageVM.ReceiverId);

            await PushNotification(usr, "You have a message", null);

            return messageVM;

        }

        [HttpGet]
        [Route("MessagesExist/{CoachId}")]
        public ActionResult<bool> MessagesExist(Guid CoachId)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var noMessages = _unitOfWork.MessageRepository.FilterBy(x => x.SenderId == user.Id && x.ReceiverId == CoachId).Count();

            return noMessages > 0;
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
                                   SentDate = msg.SentDate,
                                   //SenderName = _unitOfWork.UserRepository.FindById(msg.SenderId).FullName,
                                   //SenderProfilePic = _unitOfWork.UserRepository.FindById(msg.SenderId).ProfileImage,
                               }).OrderByDescending(x => x.SentDate).FirstOrDefault();
                if (message != null)
                {
                    var user = _unitOfWork.UserRepository.FindById(message.SenderID);
                    if (user != null)
                    {
                        message.SenderName = user.FullName;
                        message.SenderProfilePic = user.ProfileImage;
                    }
                    message.ReceiverProfilePic = string.IsNullOrEmpty(message.ReceiverProfilePic) ? "" : ((message.ReceiverProfilePic.Contains("http://") || message.ReceiverProfilePic.Contains("https://")) ? message.ReceiverProfilePic : _jwtAppSettings.AppBaseURL + message.ReceiverProfilePic);
                    message.SenderProfilePic = string.IsNullOrEmpty(message.SenderProfilePic) ? "" : ((message.SenderProfilePic.Contains("http://") || message.SenderProfilePic.Contains("https://")) ? message.SenderProfilePic : _jwtAppSettings.AppBaseURL + message.SenderProfilePic);

                    message.Sender = _unitOfWork.UserRepository.FilterBy(x => x.Id == message.SenderID).Select(x => new UserDataViewModel()
                    {
                        Id = x.Id,
                        Role = x.Role,
                        Address = x.Address,
                        EmailID = x.EmailID,
                        FullName = x.FullName,
                        MobileNo = x.MobileNo,
                        ProfileImage = x.ProfileImage,
                        DBSCeritificate = x.DBSCeritificate,
                        Teams = x.Teams,
                        Qualifications = x.Qualifications,
                        TrainingLocations = x.TrainingLocations,
                        Rate = x.Rate,
                        VerificationDocument = x.VerificationDocument,
                        PostCode = x.PostCode,
                        AboutUs = x.AboutUs,
                        Achievements = x.Achievements,
                        Accomplishment = x.Accomplishment,
                        Lat = x.Lat,
                        Lng = x.Lng,
                        TravelMile = x.TravelMile,
                        HiddenPosts = x.HiddenPosts,
                        Experiences = x.Experiences

                    }).SingleOrDefault();

                    if (message.Sender != null)
                    {
                        message.Sender.ProfileImage = string.IsNullOrEmpty(message.Sender.ProfileImage) ? "" : ((message.Sender.ProfileImage.Contains("http://") || message.Sender.ProfileImage.Contains("https://")) ? message.Sender.ProfileImage : _jwtAppSettings.AppBaseURL + message.Sender.ProfileImage);
                    }
                    messages.Add(message);
                }
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
        public ActionResult<Booking> SaveBooking(BookingDataViewModel booking)
        {
            var user = _unitOfWork.UserRepository.FindById(booking.PlayerID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == booking.CoachID);
            foreach (var session in booking.Sessions)
            {
                string bookDate = session.BookingDate.ToString("yyyy-MM-dd");
                DateTime fromTime = DateTime.ParseExact(bookDate + " " + session.FromTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
                DateTime toTime = DateTime.ParseExact(bookDate + " " + session.ToTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);

                foreach(var bookingItem in bookings)
                {
                   var checkTimes =  bookingItem.Sessions.Where(x => x.FromTime >= fromTime && x.ToTime <= toTime).ToList();
                    if (checkTimes.Count() > 0)
                    {
                        return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "Booking already exists." } } });
                    }
                }
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
            else
            {
                coach = new Coach();
                coach.CoachId = booking.CoachID;
                coach.Status = "Hired";
                user.Coaches.Add(coach);
                _unitOfWork.UserRepository.ReplaceOne(user);
            }


            var availableSessions = booking.Sessions.Select(x => new BookingTime {
                    BookingDate = x.BookingDate,
                    FromTime = DateTime.ParseExact(x.BookingDate.ToString("yyyy-MM-dd") + " " + x.FromTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture),
                    ToTime = DateTime.ParseExact(x.BookingDate.ToString("yyyy-MM-dd") + " " + x.ToTime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture)
            }).ToList();


            Booking book = new Booking();
            book.Id = Guid.NewGuid();
            book.Amount = booking.Amount;
            book.BookingStatus = booking.BookingStatus;
            book.PaymentStatus = booking.PaymentStatus;
            book.PlayerID = booking.PlayerID;
            book.TrainingLocationID = booking.TrainingLocationID;
            book.CoachID = booking.CoachID;
            book.TransactionID = booking.TransactionID;
            book.Sessions = availableSessions;
            book.SentDate = DateTime.Now;
            if (_unitOfWork.BookingRepository.AsQueryable().Count() > 0)
            {
                book.BookingNumber = _unitOfWork.BookingRepository.AsQueryable().Select(x => x.BookingNumber).Max() + 1;
            }
            else
            {
                book.BookingNumber = 1;
            }
            _unitOfWork.BookingRepository.InsertOne(book);

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = "New booking received.";
            notification.CreatedDate = DateTime.Now;
            notification.UserId = booking.CoachID;
            _unitOfWork.NotificationRepository.InsertOne(notification);

            var usr = _unitOfWork.UserRepository.FindById(booking.PlayerID);

            var mailBookingSessions = string.Empty;

            foreach (var session in booking.Sessions)
                mailBookingSessions += session.BookingDate.ToString("dd MMM yyyy") + " " + session.FromTime + " - " + session.ToTime + ",";

            mailBookingSessions = mailBookingSessions.Substring(0, mailBookingSessions.Length - 1);
            var values = new Dictionary<string, string>();
            values.Add("BookingDate", mailBookingSessions);
            EmailHelper.SendEmail(usr.EmailID, _emailSettings, "booking", values);

            var coachUser = _unitOfWork.UserRepository.FindById(booking.CoachID);
            if (coachUser.DeviceType != null && Convert.ToString(coachUser.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                AndriodPushNotification(coachUser.DeviceToken, notification);
            }
            else if (coachUser.DeviceType != null && Convert.ToString(coachUser.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                 ApplePushNotification(coachUser.DeviceToken, notification);
            }
            return book;

        }

        [HttpGet]
        [Route("CancelBooking/{BookingId}")]
        public ActionResult<Booking> CancelBooking(Guid bookingID)
        {

            var booking = _unitOfWork.BookingRepository.FindById(bookingID);
            booking.BookingStatus = "Cancelled";
            booking.Sessions.ForEach(x => x.Status = "Cancelled");
            booking.CancelledDateTime = DateTime.Now;
            _unitOfWork.BookingRepository.ReplaceOne(booking);

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = "Booking cancelled successfully.";
            notification.CreatedDate = DateTime.Now;
            notification.UserId = _userContext.UserID;
            _unitOfWork.NotificationRepository.InsertOne(notification);

            var user = _unitOfWork.UserRepository.FindById(booking.PlayerID);
            EmailHelper.SendEmail(user.EmailID, _emailSettings, "cancelbooking");

            var player = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (player.DeviceType != null && Convert.ToString(player.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                AndriodPushNotification(player.DeviceToken, notification);
            }
            else if (player.DeviceType != null && Convert.ToString(player.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                 ApplePushNotification(user.DeviceToken, notification);
            }
            return booking;

        }

        [HttpPost]
        [Route("GetBookings")]
        public ActionResult<List<BookingViewModel>> GetBookings(BookingFilterViewModel booking)
        {
            List<BookingViewModel> bookings = new List<BookingViewModel>();
            if(booking.StartDate.HasValue && booking.EndDate.HasValue)
            {
                booking.StartDate = new DateTime(booking.StartDate.Value.Year, booking.StartDate.Value.Month, booking.StartDate.Value.Day, 0, 0, 0);
                booking.EndDate = new DateTime(booking.EndDate.Value.Year, booking.EndDate.Value.Month, booking.EndDate.Value.Day, 23, 59, 59);
            }


            
            if (booking.Role.ToLower() == Constants.COACH)
            {
                var tempBooking = new List<Booking>();
                var matchedBookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == booking.UserID).ToList();
                if (booking.StartDate.HasValue && booking.EndDate.HasValue)
                {
                    foreach(var matchedBooking in matchedBookings)
                    {
                        var matched = false;
                        foreach(var session in matchedBooking.Sessions)
                        {
                            if (session.BookingDate >= booking.StartDate.Value && session.BookingDate <= booking.EndDate.Value)
                            {
                                matched = true;
                                break;
                            }
                                
                        }
                        if (matched)
                            tempBooking.Add(matchedBooking);
                    }
                    matchedBookings = tempBooking;
                }
              

                bookings = matchedBookings.Select(x => new
                BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    TransactionID = x.TransactionID,
                    FromTime = x.FromTime,
                    ToTime = x.ToTime,
                    BookingDate = x.BookingDate,
                    CancelledDateTime = x.CancelledDateTime,
                    RescheduledDateTime = x.RescheduledDateTime,
                    CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                    Player = new PlayerVM()
                    {
                        FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                        ProfileImage = _unitOfWork.UserRepository.FindById(x.PlayerID).ProfileImage,
                        AboutUs = _unitOfWork.UserRepository.FindById(x.PlayerID).AboutUs,
                        Achievements = _unitOfWork.UserRepository.FindById(x.PlayerID).Achievements,
                        Teams = _unitOfWork.UserRepository.FindById(x.PlayerID).Teams,
                        UpcomingMatches = _unitOfWork.UserRepository.FindById(x.PlayerID).UpcomingMatches,
                        Address = _unitOfWork.UserRepository.FindById(x.PlayerID).Address
                    },
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                    CurrentTime = DateTime.Now,
                    Sessions = x.Sessions.Select(x => new BookingTimeViewModel {
                        BookingDate = x.BookingDate,
                        ToTime = x.ToTime,
                        FromTime = x.FromTime,
                        SessionStatus = x.Status
                    }).ToList(),
                    BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                    {
                        BookingId = x.Id,
                        Feedback = b.Feedback,
                        Id = b.Id,
                        PlayerId = b.PlayerId,
                        Rating = b.Rating,
                        PlayerProfileImage = _unitOfWork.UserRepository.FindById(b.PlayerId).ProfileImage,
                        CreatedDate = b.CreatedDate
                    }).ToList()
                }
                ).ToList();

                bookings.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));
                bookings.ForEach(user => user.Player.ProfileImage = string.IsNullOrEmpty(user.Player.ProfileImage) ? "" : ((user.Player.ProfileImage.Contains("http://") || user.Player.ProfileImage.Contains("https://")) ? user.Player.ProfileImage : _jwtAppSettings.AppBaseURL + user.Player.ProfileImage));


                foreach (var book in bookings)
                {
                    book.Statuses = new List<BookingStatusViewModel>();
                    var completedSessionCount = 0;
                    var sessionStatuses = new List<BookingStatusViewModel>();
                    foreach (var session in book.Sessions)
                    {
                        var sessionTime = session.FromTime.ToString("yyyy-MM-dd hh:mmtt") + "-" + session.ToTime.ToString("hh:mmtt");
                        if (DateTime.Now >= session.FromTime && DateTime.Now <= session.ToTime)
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session In Progress", Date = sessionTime });
                        }
                        else if (DateTime.Now > session.ToTime)
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Completed", Date = sessionTime });
                            completedSessionCount++;
                        }
                        else
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Scheduled", Date = sessionTime });

                        }

                    }
                    if (book.RescheduledDateTime != null)
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Rescheduled", Date = book.RescheduledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });

                        book.Statuses.AddRange(sessionStatuses);
                    }
                    else if (book.CancelledDateTime != null)
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Cancelled", Date = book.CancelledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                    else if (completedSessionCount == book.Sessions.Count)
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Completed", Date = book.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                        book.Statuses.AddRange(sessionStatuses);

                    }
                    else
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Scheduled", Date = book.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                        book.Statuses.AddRange(sessionStatuses);
                    }
                }
            }
            else
            {
                var tempBooking = new List<Booking>();
                
                var matchedBookings = _unitOfWork.BookingRepository.FilterBy(x => x.PlayerID == booking.UserID).ToList();

                if (booking.StartDate.HasValue && booking.EndDate.HasValue)
                {
                    foreach (var matchedBooking in matchedBookings)
                    {
                        var matched = false;
                        foreach (var session in matchedBooking.Sessions)
                        {
                            if (session.BookingDate >= booking.StartDate.Value && session.BookingDate <= booking.EndDate.Value)
                            {
                                matched = true;
                                break;
                            }

                        }
                        if (matched)
                            tempBooking.Add(matchedBooking);
                    }
                    matchedBookings = tempBooking;
                }

                bookings = matchedBookings.Select(x => new
               BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FullName = _unitOfWork.UserRepository.FindById(x.CoachID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    FromTime = x.FromTime,
                    ToTime = x.ToTime,
                    BookingDate = x.BookingDate,
                    TransactionID = x.TransactionID,
                    CancelledDateTime = x.CancelledDateTime,
                    RescheduledDateTime = x.RescheduledDateTime,
                    CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                    CurrentTime = DateTime.Now,
                    Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                    {
                        SessionStatus = x.Status,
                        BookingDate = x.BookingDate,
                        ToTime = x.ToTime,
                        FromTime = x.FromTime
                    }).ToList(),
                    Address = _unitOfWork.UserRepository.FindById(x.CoachID).Address,
                    BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                    {
                        BookingId = x.Id,
                        Feedback = b.Feedback,
                        Id = b.Id,
                        PlayerId = b.PlayerId,
                        Rating = b.Rating,
                        CreatedDate = b.CreatedDate
                    }).ToList()
                }
               ).ToList();

                bookings.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));

                
                foreach (var book in bookings)
                {
                    book.Statuses = new List<BookingStatusViewModel>();
                    var completedSessionCount = 0;
                    var sessionStatuses = new List<BookingStatusViewModel>();
                    foreach (var session in book.Sessions)
                    {
                        var sessionTime = session.FromTime.ToString("yyyy-MM-dd hh:mmtt") + "-" + session.ToTime.ToString("hh:mmtt");
                        if (DateTime.Now >= session.FromTime && DateTime.Now <= session.ToTime)
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session In Progress", Date = sessionTime });
                        }
                        else if (DateTime.Now > session.ToTime)
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Completed", Date = sessionTime });
                            completedSessionCount++;
                        }
                        else
                        {
                            sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Scheduled", Date = sessionTime });

                        }

                    }
                    if (book.RescheduledDateTime != null)
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Rescheduled", Date = book.RescheduledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });

                        book.Statuses.AddRange(sessionStatuses);
                    }
                    else if (book.CancelledDateTime != null)
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Cancelled", Date = book.CancelledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                    else if (completedSessionCount == book.Sessions.Count)
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Completed", Date = book.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                        book.Statuses.AddRange(sessionStatuses);

                    }
                    else
                    {
                        book.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Scheduled", Date = book.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                        book.Statuses.AddRange(sessionStatuses);
                    }

                }
            }
            return bookings;
        }


        [HttpPost]
        [Route("GetBookingById/{bookingId}")]
        public ActionResult<BookingViewModel> GetBookingById(Guid bookingId)
        {
            var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.Id == bookingId).ToList();

            var booking = bookings.Select(x => new
                BookingViewModel()
            {
                Amount = x.Amount,
                BookingNumber = x.BookingNumber,
                BookingStatus = x.BookingStatus,
                CoachID = x.CoachID,
                FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                Id = x.Id,
                Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                TrainingLocationID = x.TrainingLocationID,
                PaymentStatus = x.PaymentStatus,
                PlayerID = x.PlayerID,
                SentDate = x.SentDate,
                FromTime = x.FromTime,
                ToTime = x.ToTime,
                BookingDate = x.BookingDate,
                TransactionID = x.TransactionID,
                CancelledDateTime = x.CancelledDateTime,
                RescheduledDateTime = x.RescheduledDateTime,
                CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                Player = new PlayerVM()
                {
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.PlayerID).ProfileImage,
                    AboutUs = _unitOfWork.UserRepository.FindById(x.PlayerID).AboutUs,
                    Achievements = _unitOfWork.UserRepository.FindById(x.PlayerID).Achievements,
                    Teams = _unitOfWork.UserRepository.FindById(x.PlayerID).Teams,
                    UpcomingMatches = _unitOfWork.UserRepository.FindById(x.PlayerID).UpcomingMatches,
                    Address = _unitOfWork.UserRepository.FindById(x.PlayerID).Address
                },
                ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                CurrentTime = DateTime.Now,
                Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                {
                    SessionStatus = x.Status,
                    BookingDate = x.BookingDate,
                    ToTime = x.ToTime,
                    FromTime = x.FromTime
                }).ToList(),
                BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                {
                    BookingId = x.Id,
                    Feedback = b.Feedback,
                    Id = b.Id,
                    PlayerId = b.PlayerId,
                    Rating = b.Rating,
                    CreatedDate = b.CreatedDate
                }).ToList()
            }
            ).SingleOrDefault();

            booking.ProfileImage = string.IsNullOrEmpty(booking.ProfileImage) ? "" : ((booking.ProfileImage.Contains("http://") || booking.ProfileImage.Contains("https://")) ? booking.ProfileImage : _jwtAppSettings.AppBaseURL + booking.ProfileImage);
            booking.Player.ProfileImage = string.IsNullOrEmpty(booking.Player.ProfileImage) ? "" : ((booking.Player.ProfileImage.Contains("http://") || booking.Player.ProfileImage.Contains("https://")) ? booking.Player.ProfileImage : _jwtAppSettings.AppBaseURL + booking.Player.ProfileImage);
            booking.Statuses = new List<BookingStatusViewModel>();
            var completedSessionCount = 0;
            var sessionStatuses = new List<BookingStatusViewModel>();
            foreach (var session in booking.Sessions)
            {
                var sessionTime = session.FromTime.ToString("yyyy-MM-dd hh:mmtt") + "-" + session.ToTime.ToString("hh:mmtt");
                if (DateTime.Now >= session.FromTime && DateTime.Now <= session.ToTime)
                {
                    sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session In Progress", Date = sessionTime });
                }
                else if (DateTime.Now > session.ToTime)
                {
                    sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Completed", Date = sessionTime });
                    completedSessionCount++;
                }
                else
                {
                    sessionStatuses.Add(new BookingStatusViewModel() { Status = "Session Scheduled", Date = sessionTime });

                }

            }
            if (booking.RescheduledDateTime != null)
            {
                booking.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Rescheduled", Date = booking.RescheduledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });

                booking.Statuses.AddRange(sessionStatuses);
            }
            else if (booking.CancelledDateTime != null)
                booking.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Cancelled", Date = booking.CancelledDateTime.Value.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
            else if (completedSessionCount == booking.Sessions.Count)
            {
                booking.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Completed", Date = booking.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                booking.Statuses.AddRange(sessionStatuses);

            }
            else
            {
                booking.Statuses.Add(new BookingStatusViewModel() { Status = "Booking Scheduled", Date = booking.SentDate.ToString("yyyy-MM-ddThh:mm:ss.sssZ") });
                booking.Statuses.AddRange(sessionStatuses);
            }

            return booking;
        }


        [HttpPost]
        [Route("RescheduleBooking")]
        public ActionResult<Booking> RescheduleBooking(RescheduleBookingViewModel booking)
        {

            var b = _unitOfWork.BookingRepository.FindById(booking.BookingId);
            b.Sessions = booking.Sessions.Select(x => new BookingTime {
                BookingDate = x.BookingDate,
                FromTime = x.FromTime,
                ToTime = x.ToTime,
                Status = "Rescheduled"
            }).ToList();
            b.BookingStatus = "Rescheduled";
            b.RescheduledDateTime = DateTime.Now;
            _unitOfWork.BookingRepository.ReplaceOne(b);

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = "Booking rescheduled successfully.";
            notification.CreatedDate = DateTime.Now;
            notification.UserId = _userContext.UserID;
            _unitOfWork.NotificationRepository.InsertOne(notification);

            var usr = _unitOfWork.UserRepository.FindById(b.PlayerID);
            //EmailHelper.SendEmail(usr.EmailID, _emailSettings, "reschedule", b.RescheduledDateTime.Value.ToString("dd MMM yyyy") + " " + booking.FromTime.ToString("hh:mm tt") + " - " + booking.ToTime.ToString("hh:mm tt"));

            var player = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (player.DeviceType != null && Convert.ToString(player.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                AndriodPushNotification(player.DeviceToken, notification);
            }
            else if (player.DeviceType != null && Convert.ToString(player.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                 ApplePushNotification(player.DeviceToken, notification);
            }
            return b;
        }

        [HttpPost]
        [Route("GetAvailableTimeByCoachId")]
        public ActionResult<List<FreeSlotsViewModel>> GetAvailableTimeByCoachId(CoachAvailabilityViewModel avalaibility)
        {
            var startDate = new DateTime(avalaibility.date.Year, avalaibility.date.Month, avalaibility.date.Day).AddTicks(1);
            var endDate = avalaibility.EndDate.HasValue ? new DateTime(avalaibility.EndDate.Value.Year, avalaibility.EndDate.Value.Month, avalaibility.EndDate.Value.Day).AddDays(1).AddTicks(-2) : new DateTime(avalaibility.date.Year, avalaibility.date.Month, avalaibility.date.Day).AddDays(1).AddTicks(-2);
            var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == avalaibility.CoachID).ToList();
            var tempBookings = new List<Booking>();
            if (avalaibility.RequestedDates.Any())
            {
                foreach(var date in avalaibility.RequestedDates)
                {
                    var sd = new DateTime(date.Year, date.Month, date.Day).AddTicks(1);
                    var ed = new DateTime(date.Year, date.Month, date.Day).AddDays(1).AddTicks(-2);
                    foreach (var booking in bookings)
                    {
                        foreach (var session in booking.Sessions)
                        {
                            if (session.FromTime >= sd && session.ToTime <= ed)
                            {
                                tempBookings.Add(booking);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
               
                foreach (var booking in bookings)
                {
                    foreach (var session in booking.Sessions)
                    {
                        if (session.FromTime >= startDate && session.ToTime <= endDate)
                        {
                            tempBookings.Add(booking);
                            break;
                        }
                    }
                }
            }
           

            bookings = tempBookings;

            var user = _unitOfWork.UserRepository.FindById(avalaibility.CoachID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            var slots = new List<Availability>();


          
           slots = user.Availabilities.Where(x => x.IsWorking == true).ToList();

            //List<string> bookedSlots = new List<string>();
            //foreach (var book in bookings)
            //{
            //    bookedSlots.Add(book.FromTime.ToString("hh:mm tt") + " - " + book.ToTime.ToString("hh:mm tt"));
            //}
            List<Availability> availableSlots = new List<Availability>();

            if (avalaibility.RequestedDates.Any())
            {
                foreach (var date in avalaibility.RequestedDates)
                {
                    string day = date.DayOfWeek.ToString().ToLower();

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
                                if (endtime > slot.ToTime)
                                {
                                    break;
                                }
                                availableSlots.Add(new Availability() { FromTime = new DateTime(date.Year, date.Month, date.Day, starttime.Hour, starttime.Minute, starttime.Second), ToTime = new DateTime(date.Year, date.Month, date.Day, endtime.Hour, endtime.Minute, endtime.Second), Day = date.ToString("yyyy-MM-ddTHH:mm:ss.sssZ") });
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                var endLimit = avalaibility.EndDate.HasValue ? avalaibility.EndDate.Value.AddDays(1) : avalaibility.date.AddDays(1);
                for (var currentDate = avalaibility.date; currentDate < endLimit; currentDate = currentDate.AddDays(1))
                {
                    string day = currentDate.DayOfWeek.ToString().ToLower();

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
                                if (endtime > slot.ToTime)
                                {
                                    break;
                                }
                                availableSlots.Add(new Availability() { FromTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, starttime.Hour, starttime.Minute, starttime.Second), ToTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, endtime.Hour, endtime.Minute, endtime.Second), Day = currentDate.ToString("yyyy-MM-ddTHH:mm:ss.sssZ") });
                            }

                            break;
                        }
                    }
                }
            }

           
            

            List<FreeSlotsViewModel> freeSlots = new List<FreeSlotsViewModel>();

            foreach (var slot in availableSlots)
            {

                if (bookings.SelectMany(x => x.Sessions).Where(x => slot.FromTime.TimeOfDay >= x.FromTime.TimeOfDay && slot.ToTime.TimeOfDay <= x.ToTime.TimeOfDay).Count() > 0)
                {

                }
                else
                {

                    freeSlots.Add(new FreeSlotsViewModel
                    {
                        FromTime  = slot.FromTime.ToString("hh:mm tt"),
                        ToTime = slot.ToTime.AddMinutes(-15).ToString("hh:mm tt"),
                        BookingDate = slot.Day,
                        TimeTag = slot.FromTime.ToString("hh:mm tt") + "-" + slot.ToTime.AddMinutes(-15).ToString("hh:mm tt")

                    });
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
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }
            user.TravelMile = travel;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return travel;

        }


        [HttpPost]
        [Route("UpdatePaymentDetails")]
        public ActionResult UpdatePaymentDetails(UpdatePaymentDetailsViewModel viewModel)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            user.PaypalPaymentId = viewModel.PaypalPaymentId;
            user.Featured = true;
            _unitOfWork.UserRepository.ReplaceOne(user);

            return Ok();
        }


        [HttpGet]
        [Route("GetTravelMile")]
        public ActionResult<TravelMiles> GetTravelMile()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            return user.TravelMile;
        }

        [HttpPost]
        [Route("SearchPost")]
        public ActionResult<SearchPostResultViewModel> SearchPost(SearchPostViewModel post)
        {
            var usr = _unitOfWork.UserRepository.FindById(_userContext.UserID);

            if (usr == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            List<Guid> hiddenPostIds = _unitOfWork.UserRepository.AsQueryable().SelectMany(x => x.HiddenPosts).Select(x => x.PostId).Distinct().ToList();
            bool isStartWithHash = post.Search.StartsWith("#");
            if (isStartWithHash)
            {
                var posts = _unitOfWork.PostRepository.AsQueryable().Where(x => x.UserId != _userContext.UserID && !hiddenPostIds.Contains(x.Id) && x.Body.Contains(post.Search)).Select(post => new PostDataViewModel()
                {
                    Body = post.Body,
                    CreatedDate = post.CreatedDate,
                    Header = post.Header,
                    Id = post.Id,
                    IsVerified = post.IsVerified,
                    Likes = post.Likes,
                    MediaURL = post.MediaURL,
                    NumberOfLikes = post.NumberOfLikes,
                    UserId = post.UserId,
                }).ToList();

                foreach (var user in posts)
                {
                    var pUser = _unitOfWork.UserRepository.FindById(user.UserId);
                    user.ProfileImage = pUser.ProfileImage;
                    user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage);
                    user.CreatedBy = pUser.FullName;
                    

                    try
                    {
                        //string path = item.MediaURL.Replace(baseUrl + "/", "");
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + user.MediaURL);
                        System.Drawing.Image img = System.Drawing.Image.FromFile(fullPath);
                        user.Height = img.Height;
                        user.Width = img.Width;
                        user.MediaURL = _jwtAppSettings.AppBaseURL + user.MediaURL;
                    }
                    catch (Exception ex)
                    {
                        user.MediaURL = _jwtAppSettings.AppBaseURL + user.MediaURL;
                    }
                }

                List<Guid> userIds = posts.Select(x => x.UserId).ToList();
                var coaches = _unitOfWork.UserRepository.FilterBy(x => x.Id != _userContext.UserID && userIds.Contains(x.Id) && x.Role.ToLower() == Constants.COACH).Select(x => new UserDataViewModel()
                {
                    Id = x.Id,
                    Role = x.Role,
                    Address = x.Address,
                    State = x.State,
                    EmailID = x.EmailID,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    ProfileImage = x.ProfileImage,
                    DBSCeritificate = x.DBSCeritificate,
                    Teams = x.Teams,
                    Qualifications = x.Qualifications,
                    TrainingLocations = x.TrainingLocations,
                    Rate = x.Rate,
                    VerificationDocument = x.VerificationDocument,
                    PostCode = x.PostCode,
                    AboutUs = x.AboutUs,
                    Featured = x.Featured,
                    Achievements = x.Achievements,
                    Accomplishment = x.Accomplishment,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    TravelMile = x.TravelMile,
                    HiddenPosts = x.HiddenPosts,
                    Experiences = x.Experiences
                    // Posts = _unitOfWork.PostRepository.FilterBy(p => p.UserId == x.Id).ToList()
                    //Bookings = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == x.Id).ToList()
                }).ToList();
                coaches.ForEach(user => user.Availabilities = _unitOfWork.UserRepository.FindById(user.Id).Availabilities.Select(x => new AvailabilityViewModel { Day = x.Day,FromTime = x.FromTime.ToString(),ToTime = x.ToTime.ToString(),IsWorking = x.IsWorking}).ToList());

                coaches.ForEach(user => user.Posts.ForEach(x => x.MediaURL = _jwtAppSettings.AppBaseURL + x.MediaURL));
                coaches.ForEach(user => user.Bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == user.Id).Select(x => new
                 BookingViewModel()
                {
                    Amount = x.Amount,
                    BookingNumber = x.BookingNumber,
                    BookingStatus = x.BookingStatus,
                    CoachID = x.CoachID,
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    Id = x.Id,
                    Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                    TrainingLocationID = x.TrainingLocationID,
                    PaymentStatus = x.PaymentStatus,
                    PlayerID = x.PlayerID,
                    SentDate = x.SentDate,
                    TransactionID = x.TransactionID,
                    CancelledDateTime = x.CancelledDateTime,
                    RescheduledDateTime = x.RescheduledDateTime,
                    CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                    Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                    {
                        SessionStatus = x.Status,
                        BookingDate = x.BookingDate,
                        ToTime = x.ToTime,
                        FromTime = x.FromTime
                    }).ToList(),
                    BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                    {
                        BookingId = x.Id,
                        Feedback = b.Feedback,
                        Id = b.Id,
                        PlayerId = b.PlayerId,
                        Rating = b.Rating,
                        PlayerProfileImage = _unitOfWork.UserRepository.FindById(b.PlayerId).ProfileImage,
                        PlayerName = _unitOfWork.UserRepository.FindById(b.PlayerId).FullName,
                        CreatedDate = b.CreatedDate
                    }).ToList()
                }
                ).ToList());
                coaches.ForEach(x => x.Bookings.ForEach(b => b.BookingReviews.ForEach(x => x.PlayerProfileImage = x.PlayerProfileImage != null ? ((x.PlayerProfileImage.Contains("http://") || x.PlayerProfileImage.Contains("https:/")) ? x.PlayerProfileImage : _jwtAppSettings.AppBaseURL + x.PlayerProfileImage) : "")));

                var players = _unitOfWork.UserRepository.FilterBy(x => x.Id != _userContext.UserID && userIds.Contains(x.Id) && x.Role.ToLower() == Constants.PLAYER).Select(x => new UserDataViewModel()
                {
                    Id = x.Id,
                    Role = x.Role,
                    Address = x.Address,
                    State = x.State,
                    EmailID = x.EmailID,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    ProfileImage = x.ProfileImage,
                    DBSCeritificate = x.DBSCeritificate,
                    Teams = x.Teams,
                    Qualifications = x.Qualifications,
                    TrainingLocations = x.TrainingLocations,
                    Rate = x.Rate,
                    Featured = x.Featured,
                    VerificationDocument = x.VerificationDocument,
                    PostCode = x.PostCode,
                    AboutUs = x.AboutUs,
                    Achievements = x.Achievements,
                    Accomplishment = x.Accomplishment,
                    Experiences = x.Experiences,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    CreatedAt = x.RegisterDate
                }).ToList();


                var featuredUsers = _unitOfWork.UserRepository.FilterBy(x => x.Id != _userContext.UserID && userIds.Contains(x.Id) && x.Featured).Select(x => new UserDataViewModel()
                {
                    Id = x.Id,
                    Role = x.Role,
                    Address = x.Address,
                    State = x.State,
                    EmailID = x.EmailID,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    ProfileImage = x.ProfileImage,
                    DBSCeritificate = x.DBSCeritificate,
                    Teams = x.Teams,
                    Qualifications = x.Qualifications,
                    TrainingLocations = x.TrainingLocations,
                    Rate = x.Rate,
                    Featured = x.Featured,
                    VerificationDocument = x.VerificationDocument,
                    PostCode = x.PostCode,
                    AboutUs = x.AboutUs,
                    Achievements = x.Achievements,
                    Accomplishment = x.Accomplishment,
                    Experiences = x.Experiences,
                    Lat = x.Lat,
                    Lng = x.Lng
                }).ToList();





                foreach (var item in coaches)
                {
                    item.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);
                    if (item.DBSCeritificate != null)
                    {
                        item.DBSCeritificate.Path = string.IsNullOrEmpty(item.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + item.DBSCeritificate.Path;
                    }

                    if (item.VerificationDocument != null)
                    {
                        item.VerificationDocument.Path = string.IsNullOrEmpty(item.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + item.VerificationDocument.Path;
                    }
                    int total = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Sum(x => x.Rating);
                    int count = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Count();
                    item.AverageBookingRating = count == 0 ? "New" : (total / count).ToString();
                    item.Bookings.ForEach(b => b.BookingReviews.ForEach(r => r.PlayerProfileImage = string.IsNullOrEmpty(r.PlayerProfileImage) ? "" : ((r.PlayerProfileImage.Contains("http://") || r.PlayerProfileImage.Contains("https://")) ? r.PlayerProfileImage : _jwtAppSettings.AppBaseURL + r.PlayerProfileImage)));


                    List<Guid> hPostIds = item.HiddenPosts.Select(x => x.PostId).ToList();
                    var psts = _unitOfWork.PostRepository.FilterBy(x => x.UserId == item.Id && !hPostIds.Contains(x.Id)).Select(post => new PostDataViewModel()
                    {
                        Body = post.Body,
                        CreatedDate = post.CreatedDate,
                        Header = post.Header,
                        Id = post.Id,
                        IsVerified = post.IsVerified,
                        Likes = post.Likes,
                        MediaURL = _jwtAppSettings.AppBaseURL + post.MediaURL,
                        NumberOfLikes = post.NumberOfLikes,
                        UserId = post.UserId
                    }).ToList();

                    foreach (var p in psts)
                    {
                        var pUser = _unitOfWork.UserRepository.FindById(p.UserId);
                        p.ProfileImage = pUser.ProfileImage;
                        p.ProfileImage = string.IsNullOrEmpty(p.ProfileImage) ? "" : ((p.ProfileImage.Contains("http://") || p.ProfileImage.Contains("https://")) ? p.ProfileImage : _jwtAppSettings.AppBaseURL + p.ProfileImage);
                        p.CreatedBy = pUser.FullName;
                    }

                    item.ProfileImage = string.IsNullOrEmpty(item.ProfileImage) ? "" : ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https://")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage);

                    item.Posts = psts;
                    var coachBookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == item.Id);
                    item.Level = coachBookings.Sum(x => x.Sessions.Where(x => x.Status == "completed").Count());
                    item.Level = Convert.ToInt32(Math.Ceiling((double)item.Level / 50));
                    if (item.Level == 0)
                    {
                        item.Level = 1;
                    }
                }
                foreach (var item in players)
                {
                    item.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);
                    if (item.DBSCeritificate != null)
                    {
                        item.DBSCeritificate.Path = string.IsNullOrEmpty(item.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + item.DBSCeritificate.Path;
                    }

                    if (item.VerificationDocument != null)
                    {
                        item.VerificationDocument.Path = string.IsNullOrEmpty(item.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + item.VerificationDocument.Path;
                    }
                    item.ProfileImage = string.IsNullOrEmpty(item.ProfileImage) ? "" : ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https://")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage);
                    //item.BookingCount = _unitOfWork.BookingRepository.FilterBy(x => x.PlayerID == item.Id && x.BookingStatus.ToLower() != "cancelled").Count();
                }

                foreach (var item in featuredUsers)
                {
                    item.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);
                    if (item.DBSCeritificate != null)
                    {
                        item.DBSCeritificate.Path = string.IsNullOrEmpty(item.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + item.DBSCeritificate.Path;
                    }

                    if (item.VerificationDocument != null)
                    {
                        item.VerificationDocument.Path = string.IsNullOrEmpty(item.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + item.VerificationDocument.Path;
                    }
                    item.ProfileImage = string.IsNullOrEmpty(item.ProfileImage) ? "" : ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https://")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage);
                    //item.BookingCount = _unitOfWork.BookingRepository.FilterBy(x => x.PlayerID == item.Id && x.BookingStatus.ToLower() != "cancelled").Count();
                }


                coaches.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));
                players.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));
                featuredUsers.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));

                SearchPostResultViewModel searchResult = new SearchPostResultViewModel();
                searchResult.Coaches = coaches;
                searchResult.Players = players;
                searchResult.Featured = featuredUsers;
                searchResult.Posts = posts;

                return searchResult;
            }
            else
            {
                string postSearch = Regex.Replace(Convert.ToString(post.Search).ToLower().Trim(), @"\s", "");
                var users = _unitOfWork.UserRepository.AsQueryable().Where(x => x.Id != _userContext.UserID && (x.FullName.ToLower().Contains(post.Search.ToLower()) || x.PostCode.ToLower().Contains(postSearch))).Select(x => new UserDataViewModel()
                {
                    Id = x.Id,
                    Role = x.Role,
                    Address = x.Address,
                    State = x.State,
                    EmailID = x.EmailID,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    Featured = x.Featured,
                    ProfileImage = x.ProfileImage,
                    DBSCeritificate = x.DBSCeritificate,
                    Teams = x.Teams,
                    Qualifications = x.Qualifications,
                    TrainingLocations = x.TrainingLocations,
                    Rate = x.Rate,
                    VerificationDocument = x.VerificationDocument,
                    PostCode = x.PostCode,
                    AboutUs = x.AboutUs,
                    Achievements = x.Achievements,
                    Accomplishment = x.Accomplishment,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    TravelMile = x.TravelMile,
                    Experiences = x.Experiences,
                    CreatedAt = x.RegisterDate
                }).ToList();

                foreach (var item in users)
                {
                     item.Bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == item.Id).Select(x => new
                BookingViewModel()
                    {
                        Amount = x.Amount,
                        BookingNumber = x.BookingNumber,
                        BookingStatus = x.BookingStatus,
                        CoachID = x.CoachID,
                        FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                        Id = x.Id,
                        Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                        TrainingLocationID = x.TrainingLocationID,
                        PaymentStatus = x.PaymentStatus,
                        PlayerID = x.PlayerID,
                        SentDate = x.SentDate,
                        TransactionID = x.TransactionID,
                        CancelledDateTime = x.CancelledDateTime,
                        RescheduledDateTime = x.RescheduledDateTime,
                        CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                        ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                        Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                        {
                            SessionStatus = x.Status,
                            BookingDate = x.BookingDate,
                            ToTime = x.ToTime,
                            FromTime = x.FromTime
                        }).ToList(),
                        BookingReviews = x.Reviews.Select(b => new BookingReviewViewModel()
                        {
                            BookingId = x.Id,
                            Feedback = b.Feedback,
                            Id = b.Id,
                            PlayerId = b.PlayerId,
                            Rating = b.Rating,
                            PlayerProfileImage = _unitOfWork.UserRepository.FindById(b.PlayerId).ProfileImage,
                            PlayerName = _unitOfWork.UserRepository.FindById(b.PlayerId).FullName,
                            CreatedDate = b.CreatedDate
                        }).ToList()
                    }
               ).ToList();
                    item.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);
                    if (item.DBSCeritificate != null)
                    {
                        item.DBSCeritificate.Path = string.IsNullOrEmpty(item.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + item.DBSCeritificate.Path;
                    }

                    if (item.VerificationDocument != null)
                    {
                        item.VerificationDocument.Path = string.IsNullOrEmpty(item.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + item.VerificationDocument.Path;
                    }
                    item.ProfileImage = string.IsNullOrEmpty(item.ProfileImage) ? "" : ((item.ProfileImage.Contains("http://") || item.ProfileImage.Contains("https://")) ? item.ProfileImage : _jwtAppSettings.AppBaseURL + item.ProfileImage);
                    //item.Posts = _unitOfWork.PostRepository.FilterBy(p => p.UserId == item.Id).ToList();
                }

                users.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));
                users.ForEach(user => user.Posts.ForEach(x => x.MediaURL = _jwtAppSettings.AppBaseURL + x.MediaURL));
                var players = users.Where(x => x.Role.ToLower() == Constants.PLAYER).ToList();
                var coaches = users.Where(x => x.Role.ToLower() == Constants.COACH).ToList();
                var featuredUsers = users.Where(x => x.Featured).ToList();

                coaches.ForEach(x => x.Bookings.ForEach(b => b.BookingReviews.ForEach(x => x.PlayerProfileImage = x.PlayerProfileImage != null ? ((x.PlayerProfileImage.Contains("http://") || x.PlayerProfileImage.Contains("https:/")) ? x.PlayerProfileImage : _jwtAppSettings.AppBaseURL + x.PlayerProfileImage) : "")));
                foreach (var item in coaches)
                {
                    int total = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Sum(x => x.Rating);
                    int count = _unitOfWork.BookingRepository.FilterBy(b => b.CoachID == item.Id).SelectMany(r => r.Reviews).Count();
                    item.AverageBookingRating = count == 0 ? "New" : (total / count).ToString();
                    item.Bookings.ForEach(b => b.BookingReviews.ForEach(r => r.PlayerProfileImage = string.IsNullOrEmpty(r.PlayerProfileImage) ? "" : ((r.PlayerProfileImage.Contains("http://") || r.PlayerProfileImage.Contains("https://")) ? r.PlayerProfileImage : _jwtAppSettings.AppBaseURL + r.PlayerProfileImage)));
                    
                    List<Guid> hPostIds = item.HiddenPosts.Select(x => x.PostId).ToList();
                    var psts = _unitOfWork.PostRepository.FilterBy(x => x.UserId == item.Id && !hPostIds.Contains(x.Id)).Select(post => new PostDataViewModel()
                    {
                        Body = post.Body,
                        CreatedDate = post.CreatedDate,
                        Header = post.Header,
                        Id = post.Id,
                        IsVerified = post.IsVerified,
                        Likes = post.Likes,
                        MediaURL = _jwtAppSettings.AppBaseURL + post.MediaURL,
                        NumberOfLikes = post.NumberOfLikes,
                        UserId = post.UserId
                    }).ToList();
                    item.Availabilities = _unitOfWork.UserRepository.FindById(item.Id).Availabilities.Select(x => new AvailabilityViewModel { Day = x.Day, FromTime = x.FromTime.ToString(), ToTime = x.ToTime.ToString(), IsWorking = x.IsWorking }).ToList();

                    foreach (var p in psts)
                    {
                        var pUser = _unitOfWork.UserRepository.FindById(p.UserId);
                        p.ProfileImage = pUser.ProfileImage;
                        p.ProfileImage = string.IsNullOrEmpty(p.ProfileImage) ? "" : ((p.ProfileImage.Contains("http://") || p.ProfileImage.Contains("https://")) ? p.ProfileImage : _jwtAppSettings.AppBaseURL + p.ProfileImage);
                        p.CreatedBy = pUser.FullName;
                    }
                    item.Posts = psts;
                    item.Level = Convert.ToInt32(Math.Ceiling((double)item.Level / 50));
                    if (item.Level == 0)
                    {
                        item.Level = 1;
                    }
                }

                //foreach (var item in players)
                //{
                //    item.BookingCount = _unitOfWork.BookingRepository.FilterBy(x => x.PlayerID == item.Id && x.BookingStatus.ToLower() != "cancelled").Count();
                //}

                List<Guid> userIds = users.Select(x => x.Id).ToList();
                var posts = _unitOfWork.PostRepository.FilterBy(x => x.UserId != _userContext.UserID && !hiddenPostIds.Contains(x.Id) && userIds.Contains(x.Id)).Select(post => new PostDataViewModel()
                {
                    Body = post.Body,
                    CreatedDate = post.CreatedDate,
                    Header = post.Header,
                    Id = post.Id,
                    IsVerified = post.IsVerified,
                    Likes = post.Likes,
                    MediaURL = post.MediaURL,
                    NumberOfLikes = post.NumberOfLikes,
                    UserId = post.UserId
                }).ToList();

                foreach (var user in posts)
                {
                    var pUser = _unitOfWork.UserRepository.FindById(user.UserId);
                    user.ProfileImage = pUser.ProfileImage;
                    user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage);
                    user.CreatedBy = pUser.FullName;
                    
                    try
                    {
                        //string path = item.MediaURL.Replace(baseUrl + "/", "");
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + user.MediaURL);
                        System.Drawing.Image img = System.Drawing.Image.FromFile(fullPath);
                        user.Height = img.Height;
                        user.Width = img.Width;
                        user.MediaURL = _jwtAppSettings.AppBaseURL + user.MediaURL;
                    }
                    catch (Exception ex)
                    {
                        user.MediaURL = _jwtAppSettings.AppBaseURL + user.MediaURL;
                    }
                }

                SearchPostResultViewModel searchResult = new SearchPostResultViewModel();
                searchResult.Coaches = coaches;
                searchResult.Players = players;
                searchResult.Featured = featuredUsers;
                searchResult.Posts = posts;

                return searchResult;
            }
        }

        [HttpPost]
        [Route("HidePost")]
        public ActionResult<bool> HidePost(HidePostViewModel post)
        {
            var user = _unitOfWork.UserRepository.FindById(post.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            if (user.HiddenPosts.Count(x => x.PostId == post.PostId) == 0)
            {
                user.HiddenPosts.Add(new HiddenPosts() { PostId = post.PostId });
            }
            _unitOfWork.UserRepository.ReplaceOne(user);

            return true;
        }

        [HttpPost]
        [Route("ConnectUser")]
        public ActionResult<bool> ConnectUser(ConnectUserViewModel connectedUser)
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            if (connectedUser.IsConnected)
            {
                if (user.ConnectedUsers.Count(x => x.UserId == connectedUser.UserId) == 0)
                {
                    user.ConnectedUsers.Add(new ConnectedUsers() { UserId = connectedUser.UserId });
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "Connected with " + user.FullName + " successfully.";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = connectedUser.UserId;
                    _unitOfWork.NotificationRepository.InsertOne(notification);

                    var cUser = _unitOfWork.UserRepository.FindById(connectedUser.UserId);
                    if (cUser.DeviceType != null && cUser.DeviceType.ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        AndriodPushNotification(cUser.DeviceToken, notification);
                    }
                    else if (cUser.DeviceType != null && Convert.ToString(cUser.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                         ApplePushNotification(cUser.DeviceToken, notification);
                    }

                }
                var aUser = _unitOfWork.UserRepository.FindById(connectedUser.UserId);
                if (aUser != null)
                {
                    if (aUser.ConnectedUsers.Count(x => x.UserId == _userContext.UserID) == 0)
                    {
                        aUser.ConnectedUsers.Add(new ConnectedUsers() { UserId = _userContext.UserID });
                        Notification notification = new Notification();
                        notification.Id = Guid.NewGuid();
                        notification.Text = "Connected with " + aUser.FullName + " successfully.";
                        notification.CreatedDate = DateTime.Now;
                        notification.UserId = user.Id;
                        _unitOfWork.NotificationRepository.InsertOne(notification);

                        if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                        {
                            AndriodPushNotification(user.DeviceToken, notification);
                        }
                        else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                        {
                             ApplePushNotification(user.DeviceToken, notification);
                        }
                    }
                }
                _unitOfWork.UserRepository.ReplaceOne(user);

                _unitOfWork.UserRepository.ReplaceOne(aUser);
            }
            else
            {
                var toRemove = user.ConnectedUsers.Where(x => x.UserId == connectedUser.UserId).SingleOrDefault();
                if (toRemove != null)
                {
                    user.ConnectedUsers.Remove(toRemove);

                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "Disconnected with " + user.FullName + " successfully.";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = connectedUser.UserId;
                    _unitOfWork.NotificationRepository.InsertOne(notification);

                    var cUser = _unitOfWork.UserRepository.FindById(connectedUser.UserId);
                    if (cUser.DeviceType != null && Convert.ToString(cUser.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        AndriodPushNotification(cUser.DeviceToken, notification);
                    }
                    else if (cUser.DeviceType != null && Convert.ToString(cUser.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                         ApplePushNotification(cUser.DeviceToken, notification);
                    }
                }

                var aUser = _unitOfWork.UserRepository.FindById(connectedUser.UserId);
                if (aUser != null)
                {
                    var toRemovee = aUser.ConnectedUsers.Where(x => x.UserId == _userContext.UserID).SingleOrDefault();
                    if (toRemovee != null)
                    {
                        aUser.ConnectedUsers.Remove(toRemovee);

                        Notification notification = new Notification();
                        notification.Id = Guid.NewGuid();
                        notification.Text = "Disconnected with " + aUser.FullName + " successfully.";
                        notification.CreatedDate = DateTime.Now;
                        notification.UserId = user.Id;
                        _unitOfWork.NotificationRepository.InsertOne(notification);

                        if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                        {
                            AndriodPushNotification(user.DeviceToken, notification);
                        }
                        else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                        {
                             ApplePushNotification(user.DeviceToken, notification);
                        }
                    }
                }
                _unitOfWork.UserRepository.ReplaceOne(user);

                _unitOfWork.UserRepository.ReplaceOne(aUser);
            }

            return true;
        }

        [HttpGet]
        [Route("GetConnectedUsers")]
        public ActionResult<List<SearchUserViewModel>> GetConnectedUsers()
        {
            var user = _unitOfWork.UserRepository.FindById(_userContext.UserID);
            if (user == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            List<Guid> ids = user.ConnectedUsers.Select(x => x.UserId).ToList();
            var connectedUsers = _unitOfWork.UserRepository.FilterBy(x => ids.Contains(x.Id)).Select(x => new SearchUserViewModel()
            {
                Id = x.Id,
                Role = x.Role,
                Address = x.Address,
                EmailID = x.EmailID,
                FullName = x.FullName,
                MobileNo = x.MobileNo,
                ProfileImage = x.ProfileImage,
                PostCode = x.PostCode
            }).ToList();

            connectedUsers.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));

            return connectedUsers;
        }

        [HttpGet]
        [Route("GetHashTags")]
        public ActionResult<List<HashTag>> GetHashTags()
        {
            var hashTags = _unitOfWork.HashTagRepository.AsQueryable().ToList();

            return hashTags;
        }

        [HttpGet]
        [Route("GetCoachSummary/{CoachId}")]
        public ActionResult<CoachSummaryViewModel> GetCoachSummary(Guid CoachId)
        {
            DateTime midnight = DateTime.Now.Date;
            var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == CoachId).Select(x => new
                BookingViewModel()
            {
                Amount = x.Amount,
                BookingNumber = x.BookingNumber,
                BookingStatus = x.BookingStatus,
                CoachID = x.CoachID,
                FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                Id = x.Id,
                Location = _unitOfWork.UserRepository.AsQueryable().SelectMany(z => z.TrainingLocations).Where(t => t.Id == x.TrainingLocationID).FirstOrDefault(),
                TrainingLocationID = x.TrainingLocationID,
                PaymentStatus = x.PaymentStatus,
                PlayerID = x.PlayerID,
                SentDate = x.SentDate,
                Sessions = x.Sessions.Select(x => new BookingTimeViewModel
                {
                    SessionStatus = x.Status,
                    BookingDate = x.BookingDate,
                    ToTime = x.ToTime,
                    FromTime = x.FromTime
                }).ToList(),
                TransactionID = x.TransactionID,
                CancelledDateTime = x.CancelledDateTime,
                RescheduledDateTime = x.RescheduledDateTime,
                CoachRate = _unitOfWork.UserRepository.FindById(x.CoachID).Rate,
                Player = new PlayerVM()
                {
                    FullName = _unitOfWork.UserRepository.FindById(x.PlayerID).FullName,
                    ProfileImage = _unitOfWork.UserRepository.FindById(x.PlayerID).ProfileImage,
                    AboutUs = _unitOfWork.UserRepository.FindById(x.PlayerID).AboutUs,
                    Achievements = _unitOfWork.UserRepository.FindById(x.PlayerID).Achievements,
                    Teams = _unitOfWork.UserRepository.FindById(x.PlayerID).Teams,
                    UpcomingMatches = _unitOfWork.UserRepository.FindById(x.PlayerID).UpcomingMatches,
                    Address = _unitOfWork.UserRepository.FindById(x.PlayerID).Address
                },
                ProfileImage = _unitOfWork.UserRepository.FindById(x.CoachID).ProfileImage,
                CurrentTime = DateTime.Now
            }
                ).ToList();
            List<Guid> playerIds = bookings.Select(x => x.PlayerID).ToList();
            CoachSummaryViewModel cs = new CoachSummaryViewModel();
            cs.BookingsCount = bookings.Where(x => x.BookingStatus.ToLower() == "done" && x.Sessions.Any(y => y.BookingDate > midnight)).Select(x => x.PlayerID).Distinct().Count();
            cs.TotalBookingsCount = bookings.Where(x => x.BookingStatus.ToLower() == "done").Select(x => x.PlayerID).Distinct().Count();
            cs.Players = _unitOfWork.UserRepository.FilterBy(x => playerIds.Contains(x.Id)).Select(x => new UserDataViewModel()
            {
                Id = x.Id,
                Role = x.Role,
                Address = x.Address,
                EmailID = x.EmailID,
                FullName = x.FullName,
                MobileNo = x.MobileNo,
                ProfileImage = x.ProfileImage,
                DBSCeritificate = x.DBSCeritificate,
                Teams = x.Teams,
                Qualifications = x.Qualifications,
                TrainingLocations = x.TrainingLocations,
                Rate = x.Rate,
                VerificationDocument = x.VerificationDocument,
                PostCode = x.PostCode,
                AboutUs = x.AboutUs,
                Achievements = x.Achievements,
                Accomplishment = x.Accomplishment

            }).ToList();

            cs.Level = Convert.ToInt32(Math.Ceiling((double)bookings.Count(x => x.BookingStatus.ToLower() == "completed") / 50));
            if (cs.Level == 0)
            {
                cs.Level = 1;
            }

            cs.Bookings = bookings;

            cs.Players.ForEach(user => user.ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "" : ((user.ProfileImage.Contains("http://") || user.ProfileImage.Contains("https://")) ? user.ProfileImage : _jwtAppSettings.AppBaseURL + user.ProfileImage));

            foreach (var item in cs.Players)
            {
                item.TrainingLocations.ForEach(x => x.ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : _jwtAppSettings.AppBaseURL + x.ImageUrl);
                if (item.DBSCeritificate != null)
                {
                    item.DBSCeritificate.Path = string.IsNullOrEmpty(item.DBSCeritificate.Path) ? "" : _jwtAppSettings.AppBaseURL + item.DBSCeritificate.Path;
                }

                if (item.VerificationDocument != null)
                {
                    item.VerificationDocument.Path = string.IsNullOrEmpty(item.VerificationDocument.Path) ? "" : _jwtAppSettings.AppBaseURL + item.VerificationDocument.Path;
                }
            }

            return cs;
        }

        [HttpGet]
        [Route("GetNotifications")]
        public ActionResult<NotificationDataViewModel> GetNotifications()
        {
            var notifications = _unitOfWork.NotificationRepository.FilterBy(x => x.UserId == _userContext.UserID).ToList();
            NotificationDataViewModel not = new NotificationDataViewModel();
            not.Notifications = notifications;
            not.UnReadCount = notifications.Count(x => x.IsRead == false);
            return not;
        }

        [HttpGet]
        [Route("ReadNotification/{notificationId}")]
        public ActionResult<Notification> ReadNotification(Guid notificationId)
        {
            var notification = _unitOfWork.NotificationRepository.FindById(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _unitOfWork.NotificationRepository.ReplaceOne(notification);
            }

            return notification;
        }
        private void CompressImage(string path, IFormFile file)
        {
            //var optimizer = new ImageOptimizer();
            //optimizer.Compress(path);
            // Encoder parameter for image quality 
            if (file.ContentType.Contains("jpeg") || file.ContentType.Contains("jpg"))
            {
                System.IO.File.Delete(path);
                Image img = Image.FromStream(file.OpenReadStream());
                System.Drawing.Imaging.Encoder myEncoder =
                    System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                // JPEG image codec 
                ImageCodecInfo jpegCodec = GetEncoderInfo(file.ContentType);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                img.Save(path, jpegCodec, myEncoderParameters);
            }

        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

        [HttpPost]
        [Route("SaveBookingReview")]
        public ActionResult<BookingReviewViewModel> SaveBookingReview(BookingReviewViewModel reviewVM)
        {
            var booking = _unitOfWork.BookingRepository.FilterBy(x => x.Id == reviewVM.BookingId).SingleOrDefault();
            if (booking == null)
            {
                return Unauthorized(new ErrorViewModel() { errors = new Error() { error = new string[] { "User not found." } } });
            }

            var existReview = booking.Reviews.Find(x => x.Id == reviewVM.Id);
            if (existReview != null)
            {
                existReview.PlayerId = reviewVM.PlayerId;
                existReview.Rating = reviewVM.Rating;
                existReview.Feedback = reviewVM.Feedback;
                existReview.CreatedDate = DateTime.Now;

                var toRemove = booking.Reviews.Find(x => x.Id == reviewVM.Id);
                booking.Reviews.Remove(toRemove);
                booking.Reviews.Add(existReview);

            }
            else
            {
                var review = new DAL.Entities.Review()
                {
                    Id = Guid.NewGuid(),
                    PlayerId = reviewVM.PlayerId,
                    Rating = reviewVM.Rating,
                    Feedback = reviewVM.Feedback,
                    CreatedDate = DateTime.Now
                };
                booking.Reviews.Add(review);
            }


            _unitOfWork.BookingRepository.ReplaceOne(booking);

            return reviewVM;

        }

        [HttpGet]
        [Route("SendEmail/{{EmailID}}/{{LeadEmailID}}")]
        public ActionResult<bool> SendEmail(string EmailID, string LeadEmailID)
        {
            var lead = _unitOfWork.LeadsRepository.FindOne(x => x.EmailID == LeadEmailID);

            var values = new Dictionary<string, string>
            {
                { "FullName", lead.FullName },
                { "Location", lead.Location },
                { "Phone", "**** ****" },
                { "EmailID", GetMaskedEmail(lead.EmailID) }
            };
            EmailHelper.SendEmail(EmailID, _emailSettings, "newlead", values);

            return true;
        }

        [HttpPost]
        [Route("WebFormSubmit")]
        public async Task<ActionResult<JotFormResponseModel>> WebFormSubmit()
        {

            var apiKey = "9f10a7e96bcb43860b462ccdc72e7125";
            using var httpClient = new HttpClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://eu-api.jotform.com/form/203417025701039/submissions?apiKey={apiKey}");
            var http = new HttpClient();

            using var response = await httpClient.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<JotFormResponseModel>(responseString);

            if (res.ResponseCode != 200)
            {
                return null;
            }

            foreach (var content in res.Content)
            {
                var lead = new Leads();
                var MaximumPrice = "20";
                foreach (var ans in content.Answers.Values)
                {
                    if (ans.Name == "Experience")
                    {
                        lead.Experience = ans.Answer;
                    }
                    if (ans.Name == "Age")
                    {
                        lead.Age = ans.Answer;
                    }
                    if (ans.Name == "CoachingType")
                    {
                        lead.CoachingType = ans.PrettyFormat.Split("; ").ToList();
                    }
                    if (ans.Name == "Days")
                    {
                        lead.Days = ans.PrettyFormat.Split("; ").ToList();
                    }
                    if (ans.Name == "CoachingTime")
                    {
                        lead.CoachingTime = ans.PrettyFormat.Split("; ").ToList();
                    }
                    if (ans.Name == "DaysOfWeek")
                    {
                        lead.DaysOfWeek = new List<string> { ans.Answer };
                    }
                    if (ans.Name == "county")
                    {
                        if (lead.Location != null)
                        {
                            lead.Location = $"{lead.Location} {ans.Answer}".Trim();
                        }
                        else
                        {
                            lead.Location = ans.Answer;
                        }
                    }
                    if (ans.Name == "district")
                    {
                        if (lead.Location != null)
                        {
                            lead.Location = $"{ans.Answer} {lead.Location}".Trim();
                        }
                        else
                        {
                            lead.Location = ans.Answer;
                        }
                    }
                    if (ans.Name == "MobileNo")
                    {
                        lead.MobileNo = ans.PrettyFormat;
                    }
                    if (ans.Name == "FullName")
                    {
                        lead.FullName = ans.PrettyFormat;
                    }
                    if (ans.Name == "EmailID")
                    {
                        lead.EmailID = ans.Answer;
                    }
                    if (ans.Name.ToLower() == "maximumprice")
                    {
                        MaximumPrice = ans.Answer;
                    }
                }
                lead.Id = Guid.NewGuid();
                lead.CreatedAt = DateTime.Now;
                lead.Web = true;
                _unitOfWork.LeadsRepository.InsertOne(lead);

                if (lead.Location != null)
                {
                    var county = lead.Location.Split(' ').Last();
                    var coaches = _unitOfWork.UserRepository.FilterBy(x => x.Role.ToLower() == Constants.COACH && x.State.Contains(county));

                    foreach (var coach in coaches)
                    {
                        try
                        {
                            await PushNotification(coach, $"{lead.FullName} is looking for Football Coaches in {lead.Location}. Earn up to £{MaximumPrice} per hour", "New Lead");

                            var values = new Dictionary<string, string>
                            {
                                { "FullName", lead.FullName },
                                { "Location", lead.Location },
                                { "Phone", lead.MobileNo != null ? GetMaskedMobileNo(lead.MobileNo) : "**** ****" },
                                { "EmailID", GetMaskedEmail(lead.EmailID) },
                                { "LatLng", $"{coach.Lat},{coach.Lng}" }
                            };
                            EmailHelper.SendEmail(coach.EmailID, _emailSettings, "newlead", values);
                        }
                        catch { }

                        
                    }
                }
            }


            foreach (var content in res.Content)
            {
                await DeleteSubmission(content.Id);
            }

            return res;
        }

        private async Task DeleteSubmission(string Id)
        {
            var apiKey = "9f10a7e96bcb43860b462ccdc72e7125";
            using var httpClient = new HttpClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"https://eu-api.jotform.com/submission/{Id}?apiKey={apiKey}");
            var http = new HttpClient();

            await httpClient.SendAsync(httpRequest);
        }


        [HttpGet]
        [Route("UpdateUserStates/{Role}")]
        public async Task<ActionResult<List<Users>>> UpdateUserStates(string Role)
        {
            var players = _unitOfWork.UserRepository.FilterBy(x => x.Role == Role).ToList();

            foreach (var player in players)
            {
                if (player.PostCode != null)
                {
                    try
                    {
                        var address = await GetAddress(player.PostCode);
                        player.State = address;
                        _unitOfWork.UserRepository.ReplaceOne(player);
                    }
                    catch { }
                }
            }

            return players;
        }


        [HttpPost]
        [Route("PayWithStripe")]
        public ActionResult<PaymentIntentViewModel> PayWithStripe(PayWithStripeViewModel data)
        {
            StripeConfiguration.ApiKey = "sk_live_MGop5tSgyzbBJyM94eMckWK800jIu8uQQb";

            var options = new PaymentIntentCreateOptions
            {
                Amount = data.Amount,
                Currency = data.Currency,
                PaymentMethodTypes = new List<string> { "card" },
                StatementDescriptor = data.StatementDescriptor,
            };

            var service = new PaymentIntentService();
            var intent = service.Create(options);

            return new PaymentIntentViewModel
            {
                Id = intent.Id,
                Amount = intent.Amount,
                ClientSecret = intent.ClientSecret,
                Status = intent.Status,
            };
        }

        private async Task<string> GetAddress(string postCode)
        {
            var apiKey = "ak_kgpgg5sceGe2S9cpVSSeU9UJo8YrI";
            using var httpClient = new HttpClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.ideal-postcodes.co.uk/v1/postcodes/{postCode}?api_key={apiKey}");
            var http = new HttpClient();
            using var response = await httpClient.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<PostCodesResponseModel>(responseString);
            var address = res.Result.First();

            return $"{address.District} {address.County}, {address.Country}";
        }

        private async Task AndriodPushNotification(string deviceToken, Notification notification, string title = null)
        {
                  
            GoogleNotification googleNotification = new GoogleNotification
            {
                To = deviceToken,
                Collapse_Key = "type_a",
                Data = new DataNotification
                {
                    Notification = notification
                },
                Notification = new NotificationModel
                {
                    Title = title ?? notification.Text,
                    Text = notification.Text,
                    Icon = !string.IsNullOrEmpty(notification.Image) ? notification.Image : "https://www.nextlevelfootballacademy.co.uk/wp-content/uploads/2019/06/logo.png"
                }
            };

            using var httpClient = new HttpClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            httpRequest.Headers.Add("Authorization", $"key = {_fcmSettings.ServerKey}");
            httpRequest.Headers.Add("Sender", $"id = {_fcmSettings.SenderId}");
            var http = new HttpClient();
            var json = JsonConvert.SerializeObject(googleNotification);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.SendAsync(httpRequest);
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
        }

        //[HttpGet]
        //[Route("AppleNotificaion")]
        private async Task ApplePushNotification(string deviceToken, Notification notification)
        {
            HttpClient httpClient = new HttpClient();
            ApnSettings apnSettings = new ApnSettings() { AppBundleIdentifier = "com.nextleveltraining", P8PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgZ1ugPXE4Hhh3L1embZmjfUdYBij8HbsrolZnzfR49X6gCgYIKoZIzj0DAQehRANCAARbCwj0VnMCOzw/Tyx4GsS4W+QN4LLCe6RRgIR/LZBJQqKi0q4XWg/p4Qa6JQAdKOZziemK4/dJZaqH/EFijM1S", P8PrivateKeyId = "FQ6ZXC7U8L", ServerType = ApnServerType.Production, TeamId = "Y77A2C426U" };
            AppleNotification appleNotification = new AppleNotification();
            appleNotification.Aps.AlertBody = notification.Text;
            appleNotification.Notification = JsonConvert.SerializeObject(notification);
            var apn = new ApnSender(apnSettings, httpClient);
            var result = await apn.SendAsync(appleNotification, deviceToken);
            if (!result.IsSuccess)
            {
                ErrorLog error = new ErrorLog();
                error.Id = Guid.NewGuid();
                error.Exception = JsonConvert.SerializeObject(result);
                error.StackTrace = "Apple Push Notification: " + notification.Text + " DeviceToken:" + deviceToken;
                error.CreatedDate = DateTime.Now;
                _unitOfWork.ErrorLogRepository.InsertOne(error);
            }
        }

        private async Task PushNotification(Users user, string text, string title)
        {
            Notification notification = new Notification
            {
                Id = Guid.NewGuid(),
                Text = text,
                CreatedDate = DateTime.Now,
                UserId = user.Id
            };
            _unitOfWork.NotificationRepository.InsertOne(notification);

            if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                await AndriodPushNotification(user.DeviceToken, notification, title);
            }
            else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                await ApplePushNotification(user.DeviceToken, notification);
            }
        }

        private string GetMaskedEmail(string email)
        {
            string[] parts = email.Split('@');
            string domainExt = Path.GetExtension(email);
            string result = string.Format("{0}****{1}@{2}****{3}{4}",
                parts[0][0],
                parts[0].Substring(parts[0].Length - 1),
                parts[1][0],
                parts[1].Substring(parts[1].Length - domainExt.Length - 1, 1),
                domainExt
            );

            return result;
        }

        private string GetMaskedMobileNo(string mobile)
        {
            string result = string.Format("{0}{1}{2}* ****",
                mobile[0],
                mobile[1],
                mobile[2]
            );

            return result;
        }
    }
}
