using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Models
{
    public class JWTAppSettings
    {
        public string Secret { get; set; }
        public string AppBaseURL { get; set; }
    }
}
