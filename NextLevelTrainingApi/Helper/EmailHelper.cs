using MongoDB.Bson;
using NextLevelTrainingApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Helper
{
    public static class EmailHelper
    {
        public static void SendEmail(string toEmail, EmailSettings settings, string emailType, Dictionary<string, string> values = null)
        {
            string content = "";
            string subject = "";
            switch (emailType)
            {
                case "booking":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/Booking Confirmation.html"));
                    content = content.Replace("{{BookingDate}}", values.GetValueOrDefault("BookingDate", ""));
                    subject = "Booking Confirmed";
                    break;
                case "reschedule":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/BookingReschedule.html"));
                    content = content.Replace("{{BookingDate}}", values.GetValueOrDefault("BookingDate", ""));
                    subject = "Booking Rescheduled";
                    break;
                case "cancelbooking":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/CancelBooking.html"));

                    subject = "Booking Cancelled";
                    break;
                case "resetpassword":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/Resetpassword.html"));
                    content = content.Replace("{{Password}}", values.GetValueOrDefault("Password", ""));
                    subject = "Password Reset";
                    break;
                case "signupcoach":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/SignupCoach.html"));
                    subject = "Welcome to the NextLevel";
                    break;
                case "signupplayer":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/SignupPlayer.html"));
                    subject = "Welcome to the NextLevel";
                    break;
                case "newlead":
                    content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/NewLead.html"));
                    string FullName = values.GetValueOrDefault("FullName", "");
                    string Location = values.GetValueOrDefault("Location", "");
                    string Phone = values.GetValueOrDefault("Phone", "");
                    string EmailID = values.GetValueOrDefault("EmailID", "");
                    subject = $"🔔 {FullName} is looking for Football Coaches in {Location}";
                    content = content.Replace("{{FullName}}", FullName);
                    content = content.Replace("{{Location}}", Location);
                    content = content.Replace("{{Phone}}", Phone);
                    content = content.Replace("{{EmailID}}", EmailID);
                    break;
            }

            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(settings.Email, "")
            };
            mail.To.Add(new MailAddress(toEmail));

            mail.Subject = subject;
            mail.Body = content;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = settings.Domain;
                smtp.Port = settings.Port;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(settings.Email, settings.Password);
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }

        }
    }
}
