using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CorePush.Apple;
using CorePush.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
    public class PushNotificationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FCMSettings _fcmSettings;
        public PushNotificationController(IUnitOfWork unitOfWork,
            IOptions<FCMSettings> fcmSettings)
        {
            _unitOfWork = unitOfWork;
            _fcmSettings = fcmSettings.Value;
        }


        [HttpPost("daily")]
        public async Task<IActionResult> Daily([FromBody] SchedulePushNotificationViewModel vm)
        {

            if (vm.Name != "DailyScheduledJob")
                return NotFound();

            var allUsers = _unitOfWork.UserRepository.AsQueryable().ToList();
            foreach (var user in allUsers.Where(x => x.Role.ToLower() == Constants.PLAYER))
            {

                //training today?
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "Training Today?";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = user.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(user.DeviceToken, notification);
                }
                else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(user.DeviceToken, notification);
                }

            }

            return Ok();

        }





        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SchedulePushNotificationViewModel vm)
        {
            if (vm.Name != Constants.ScheduledPushNotification)
                return NotFound();

            var now = DateTime.Now;


            var allUsers = _unitOfWork.UserRepository.AsQueryable();
            var players = allUsers.Where(x => x.Role.ToLower() == Constants.PLAYER).ToList();
            var coaches = allUsers.Where(x => x.Role.ToLower() == Constants.COACH).ToList();


            //24 hours after register
            var dayBefore = now.AddDays(-1);
            var playersWithAfterRegister = players.Where(x => x.RegisterDate.Date == dayBefore.Date).ToList();
            var playersWithAfterRegisterIds = playersWithAfterRegister.Select(x => x.Id).ToList();

            var playersWithBooking = _unitOfWork.BookingRepository.FilterBy(x => playersWithAfterRegisterIds.Contains(x.PlayerID)).Select(x => x.PlayerID).ToList();
            var playersWithoutBooking = playersWithAfterRegister.Where(x => !playersWithBooking.Contains(x.Id)).ToList();

            foreach(var id in playersWithBooking)
            {
                var user = players.FirstOrDefault(x => x.Id == id);
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "Time to train ⚽ Book your first 1 on 1 session today 🏆";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = user.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(user.DeviceToken, notification);
                }
                else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(user.DeviceToken, notification);
                }
            }


            ///3 days after registration
            dayBefore = now.AddDays(-3);

            playersWithAfterRegister = players.Where(x => x.RegisterDate.Date == dayBefore.Date).ToList();
            playersWithAfterRegisterIds = playersWithAfterRegister.Select(x => x.Id).ToList();

            playersWithBooking = _unitOfWork.BookingRepository.FilterBy(x => playersWithAfterRegisterIds.Contains(x.PlayerID)).Select(x => x.PlayerID).ToList();
            playersWithoutBooking = playersWithAfterRegister.Where(x => !playersWithBooking.Contains(x.Id)).ToList();


            foreach (var id in playersWithBooking)
            {
                var user = players.FirstOrDefault(x => x.Id == id);
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "Good players practise until they get it right. Great players practise until they never get it wrong  💪";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = user.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(user.DeviceToken, notification);
                }
                else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(user.DeviceToken, notification);
                }
            }


            //bi weekly after registration
            dayBefore = now.AddDays(-14);

            playersWithAfterRegister = players.Where(x => x.RegisterDate.Date == dayBefore.Date).ToList();
            playersWithAfterRegisterIds = playersWithAfterRegister.Select(x => x.Id).ToList();

            playersWithBooking = _unitOfWork.BookingRepository.FilterBy(x => playersWithAfterRegisterIds.Contains(x.PlayerID)).Select(x => x.PlayerID).ToList();
            playersWithoutBooking = playersWithAfterRegister.Where(x => !playersWithBooking.Contains(x.Id)).ToList();


            foreach (var id in playersWithBooking)
            {
                var user = players.FirstOrDefault(x => x.Id == id);
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "Good players practise until they get it right. Great players practise until they never get it wrong  💪";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = user.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(user.DeviceToken, notification);
                }
                else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(user.DeviceToken, notification);
                }
            }



            //everyday notifications
            foreach(var user in players)
            {
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "To improve you must train as much as possible. Book a training session today!";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = user.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(user.DeviceToken, notification);
                }
                else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(user.DeviceToken, notification);
                }
            }

            //begining of the month
            if(now.Day == 1)
            {
                foreach (var user in players)
                {
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "New month, new goals - Time to smash it 💪";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = user.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(user.DeviceToken, notification);
                    }
                    else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(user.DeviceToken, notification);
                    }
                }
            }


            //one day after session
            dayBefore = now.AddDays(-30);

            var lastBookings = _unitOfWork.BookingRepository.FilterBy(x => x.SentDate > dayBefore);
            dayBefore = DateTime.Now.AddDays(-1);
            foreach(var booking in lastBookings)
            {
                var hasEndedSession = false;
                foreach(var session in booking.Sessions)
                {
                    if(session.BookingDate < dayBefore)
                    {
                        hasEndedSession = true;
                        break;
                    }
                }

                if (hasEndedSession)
                {
                    var user = _unitOfWork.UserRepository.FindById(booking.PlayerID);
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "How was your Training? Leave your coach a review ⭐";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = user.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(user.DeviceToken, notification);
                    }
                    else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(user.DeviceToken, notification);
                    }
                }
            }

            //1 week after training

            dayBefore = now.AddDays(-60);

            lastBookings = _unitOfWork.BookingRepository.FilterBy(x => x.SentDate > dayBefore);
            dayBefore = now.AddDays(-7);
            foreach (var booking in lastBookings)
            {
                var hasEndedSession = false;
                foreach (var session in booking.Sessions)
                {
                    if (session.BookingDate < dayBefore)
                    {
                        hasEndedSession = true;
                        break;
                    }
                }

                if (hasEndedSession)
                {
                    var user = _unitOfWork.UserRepository.FindById(booking.PlayerID);
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "Ready for your next training session? ⚽";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = user.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(user.DeviceToken, notification);
                    }
                    else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(user.DeviceToken, notification);
                    }
                }
            }



            //first bookings for coaches
            foreach(var coach in coaches)
            {
                var bookings = _unitOfWork.BookingRepository.FilterBy(x => x.CoachID == coach.Id).ToList();
                if (!bookings.Any() || bookings.Count > 1)
                    continue;
                var hasEndedTraining = false;

                foreach(var session in bookings.FirstOrDefault().Sessions)
                {
                    if(session.BookingDate < DateTime.Now)
                    {
                        hasEndedTraining = true;
                        break;
                    }
                }

                if (hasEndedTraining)
                {

                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "How was your first session? Leave us a review on the app store ⭐";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = coach.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(coach.DeviceToken, notification);
                    }
                    else if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(coach.DeviceToken, notification);
                    }
                }
            }


            //weekly coachs notifications
            if(now.Day % 7 == 0)
            {
                foreach(var coach in coaches)
                {

                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "Set your availability and training locations for the week.Improve your profile";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = coach.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(coach.DeviceToken, notification);
                    }
                    else if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(coach.DeviceToken, notification);

                    }
                }
            }

            var signUpLimit = now.AddDays(-7);
            var weeklyCoaches = coaches.Where(x => x.RegisterDate.Date == signUpLimit.Date).ToList();
            foreach(var coach in weeklyCoaches)
            {
                Notification notification = new Notification();
                notification.Id = Guid.NewGuid();
                notification.Text = "Set your availability and training locations for the week.Improve your profile";
                notification.CreatedDate = DateTime.Now;
                notification.UserId = coach.Id;
                _unitOfWork.NotificationRepository.InsertOne(notification);
                if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                {
                    await AndriodPushNotification(coach.DeviceToken, notification);
                }
                else if (coach.DeviceType != null && Convert.ToString(coach.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                {
                    await ApplePushNotification(coach.DeviceToken, notification);

                }
            }



            if(now.Day % 7 == 0)
            {
                foreach (var user in allUsers)
                {
                    Notification notification = new Notification();
                    notification.Id = Guid.NewGuid();
                    notification.Text = "Post about training";
                    notification.CreatedDate = DateTime.Now;
                    notification.UserId = user.Id;
                    _unitOfWork.NotificationRepository.InsertOne(notification);
                    if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
                    {
                        await AndriodPushNotification(user.DeviceToken, notification);
                    }
                    else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
                    {
                        await ApplePushNotification(user.DeviceToken, notification);

                    }
                }
            }



            return Ok();
        }

        private async Task AndriodPushNotification(string deviceToken, Notification notification)
        {
            GoogleNotification googleNotification = new GoogleNotification
            {
                To = deviceToken.Trim(),
                Collapse_Key = "type_a",
                Data = new DataNotification
                {
                    Notification = notification
                },
                Notification = new NotificationModel
                {
                    Title = notification.Text,
                    Text = notification.Text,
                    Icon = !string.IsNullOrEmpty(notification.Image) ? notification.Image : "https://www.nextlevelfootballacademy.co.uk/wp-content/uploads/2019/06/logo.png"
                }
            };
            using (var httpClient = new HttpClient())
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
                httpRequest.Headers.Add("Authorization", $"key = {_fcmSettings.ServerKey}");
                httpRequest.Headers.Add("Sender", $"id = {_fcmSettings.SenderId}");
                var http = new HttpClient();
                var json = JsonConvert.SerializeObject(googleNotification);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var notificationError = response.Content.ReadAsStringAsync().Result;
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AndroidErrorDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email.ToString());
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("Error : " + notificationError.ToString());
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }
                else
                {
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AndroidWorkingDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email);
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }
            }
        }

        //[HttpGet]
        //[Route("AppleNotificaion")]
        private async Task ApplePushNotification(string deviceToken, Notification notification)
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                ApnSettings apnSettings = new ApnSettings() { AppBundleIdentifier = "com.nextleveltraining", P8PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgZ1ugPXE4Hhh3L1embZmjfUdYBij8HbsrolZnzfR49X6gCgYIKoZIzj0DAQehRANCAARbCwj0VnMCOzw/Tyx4GsS4W+QN4LLCe6RRgIR/LZBJQqKi0q4XWg/p4Qa6JQAdKOZziemK4/dJZaqH/EFijM1S", P8PrivateKeyId = "FQ6ZXC7U8L", ServerType = ApnServerType.Production, TeamId = "Y77A2C426U" };
                AppleNotification appleNotification = new AppleNotification();
                appleNotification.Aps.AlertBody = notification.Text;
                appleNotification.Notification = JsonConvert.SerializeObject(notification);
                var apn = new ApnSender(apnSettings, httpClient);
                var result = await apn.SendAsync(appleNotification, deviceToken.Trim());
                if (!result.IsSuccess)
                {
                    ApnSettings devApnSettings = new ApnSettings() { AppBundleIdentifier = "com.nextleveltraining", P8PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgZ1ugPXE4Hhh3L1embZmjfUdYBij8HbsrolZnzfR49X6gCgYIKoZIzj0DAQehRANCAARbCwj0VnMCOzw/Tyx4GsS4W+QN4LLCe6RRgIR/LZBJQqKi0q4XWg/p4Qa6JQAdKOZziemK4/dJZaqH/EFijM1S", P8PrivateKeyId = "FQ6ZXC7U8L", ServerType = ApnServerType.Development, TeamId = "Y77A2C426U" };
                    AppleNotification appleNotificationDev = new AppleNotification();
                    appleNotificationDev.Aps.AlertBody = notification.Text;
                    appleNotificationDev.Notification = JsonConvert.SerializeObject(notification);
                    var apnDev = new ApnSender(devApnSettings, httpClient);
                    var resultDev = await apnDev.SendAsync(appleNotificationDev, deviceToken.Trim());
                    if (!resultDev.IsSuccess)
                    {
                        var notificationError = result.Error.Reason;
                        var device_Token = deviceToken;
                        var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                        string path = Directory.GetCurrentDirectory();
                        var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                        var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                        string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleErrorDevices.txt";
                        using (StreamWriter writer = new StreamWriter(newPath, true))
                        {
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine("Email : " + email.ToString());
                            writer.WriteLine("Mobile_No : " + num);
                            writer.WriteLine("Device_Token : " + device_Token.ToString());
                            writer.WriteLine("Error : " + notificationError.ToString());
                            writer.WriteLine("Mode : Development");
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine();
                        }
                    }
                    else
                    {
                        var notificationError = result.Error.Reason;
                        var device_Token = deviceToken;
                        var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                        string path = Directory.GetCurrentDirectory();
                        var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                        var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                        string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleWorkingDevices.txt";
                        using (StreamWriter writer = new StreamWriter(newPath, true))
                        {
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine("Email : " + email.ToString());
                            writer.WriteLine("Mobile_No : " + num);
                            writer.WriteLine("Device_Token : " + device_Token.ToString());
                            writer.WriteLine("Mode : Development");
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine();
                        }
                    }
                    
                    
                }
                else
                {
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleWorkingDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email);
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("Mode : Production");
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }

            }
            catch (Exception ex)
            {
                string path = Directory.GetCurrentDirectory();
                string newPath = path + "\\wwwroot\\ErrorLogFile\\ErrorLogs.txt";
                using (StreamWriter writer = new StreamWriter(newPath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Message : " + ex.Message.ToString());
                    writer.WriteLine("Eception : " + ex.ToString());
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine();
                }

            }
        }
    }
}
