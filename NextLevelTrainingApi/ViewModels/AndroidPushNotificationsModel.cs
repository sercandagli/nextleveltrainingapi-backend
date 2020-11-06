using System;
using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Entities;

namespace NextLevelTrainingApi.ViewModels
{
    public class AndroidPushNotificationsModel
    {
        public string Title { get; set; }

        public string Image { get; set; }

        public string Message { get; set; }

        public string Collapse_Key { get; set; }

        public string Data { get; set; }

        public NotificationModel Notification { get; set; }
    }

    public class NotificationModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class DataNotification
    {
        [JsonProperty("notification")]
        public Notification Notification { get; set; }
    }
}
