using Newtonsoft.Json;

namespace NextLevelTrainingApi.ViewModels
{
    public class GoogleNotification
    {
        [JsonProperty("priority")]
        public string Priority { get; set; } = "high";

        [JsonProperty("data")]
        public AndroidPushNotificationsModel Data { get; set; }
    }
}
