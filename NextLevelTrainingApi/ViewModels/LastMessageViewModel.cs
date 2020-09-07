using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class LastMessageViewModel
    {
        public Guid MessageID { get; set; }
        public string Message { get; set; }
        public Guid RecieverID { get; set; }
        public Guid SenderID { get; set; }
        public string ReceiverName { get; set; }
        public DateTime SentDate { get; set; }
        public string ReceiverProfilePic { get; set; }
        public string SenderName { get; set; }
        public string SenderProfilePic { get; set; }
        public UserDataViewModel Sender { get; set; }
    }
}
