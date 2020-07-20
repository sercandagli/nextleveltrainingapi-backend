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
        public string RecieverName { get; set; }
        public string SentDate { get; set; }
        public string RecieverProfilePic { get; set; }
    }
}
