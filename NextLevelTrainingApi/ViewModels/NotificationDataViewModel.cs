using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class NotificationDataViewModel
    {
        public List<Notification> Notifications { get; set; }
        public int UnReadCount { get; set; }
    }
}
