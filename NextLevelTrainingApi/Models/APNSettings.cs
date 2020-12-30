using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Models
{
    public class APNSettings
    {
        public string AppBundleIdentifier { get; set; }
        public string P8PrivateKey { get; set; }
        public string P8PrivateKeyId { get; set; }
        public string ServerType { get; set; }
        public string TeamId { get; set; }
    }
}
