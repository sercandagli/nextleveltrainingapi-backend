using Newtonsoft.Json;

namespace NextLevelTrainingApi.ViewModels
{
    public class GoogleNotification
    {
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("priority")]
        public string Priority { get; set; } = "high";

        [JsonProperty("collapse_key")]
        public string Collapse_Key { get; set; }

        [JsonProperty("data")]
        public DataNotification Data { get; set; }

        [JsonProperty("notification")]
        public NotificationModel Notification { get; set; }
    }
}
