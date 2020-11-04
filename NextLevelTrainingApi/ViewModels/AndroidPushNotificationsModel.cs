using System;
namespace NextLevelTrainingApi.ViewModels
{
    public class AndroidPushNotificationsModel
    {
        public string Title { get; set; }

        public string Image { get; set; }

        public string Message { get; set; }

        public string Collapse_Key { get; set; }

        public NotificationModel Notification { get; set; }
    }

    public class NotificationModel
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public string Icon { get; set; }
    }
}
