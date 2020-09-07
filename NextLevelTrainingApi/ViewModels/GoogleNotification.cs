using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class GoogleNotification
    {
        public GoogleNotification()
        {
            this.Data = new DataPayload();
        }
        public class DataPayload
        {
            // Add your custom properties as needed
            //[JsonProperty("message")]
            //public string Message { get; set; }
            [JsonProperty("notification")]
            public Notification Notification { get; set; }
        }

        [JsonProperty("priority")]
        public string Priority { get; set; } = "high";

        [JsonProperty("data")]
        public DataPayload Data { get; set; }
    }
}
