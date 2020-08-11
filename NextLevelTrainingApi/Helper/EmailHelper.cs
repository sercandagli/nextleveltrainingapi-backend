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
        public static void SendEmail(string toEmail, EmailSettings settings, string emailType, string value = "")
        {
            string content = "";
            string subject = "";
            switch (emailType)
            {
                case "booking":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/Booking Confirmation.html"));
                    content = content.Replace("{{BookingDate}}", value);
                    subject = "Booking Confirmed";
                    break;
                case "reschedule":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/BookingReschedule.html"));
                    content = content.Replace("{{BookingDate}}", value);
                    subject = "Booking Rescheduled";
                    break;
                case "cancelbooking":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/CancelBooking.html"));

                    subject = "Booking Cancelled";
                    break;
                case "resetpassword":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/Resetpassword.html"));
                    content = content.Replace("{{Password}}", value);
                    subject = "Password Reset";
                    break;
                case "signupcoach":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/SignupCoach.html"));
                    subject = "Welcome to the NextLevel";
                    break;
                case "signupplayer":
                    content = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/SignupPlayer.html"));
                    subject = "Welcome to the NextLevel";
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
