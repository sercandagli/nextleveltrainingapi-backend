using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class AppleNotification
    {
        public AppleNotification()
        {
            this.Aps = new ApsPayload();
        }
        public class ApsPayload
        {
            [JsonProperty("alert")]
            public string AlertBody { get; set; }

           
        }

        // Your custom properties as needed

        [JsonProperty("aps")]
        public ApsPayload Aps { get; set; }

        [JsonProperty("notification")]
        public string Notification { get; set; }
    }
}
