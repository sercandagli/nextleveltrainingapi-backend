using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Models
{
    public class EmailSettings
    {
        public String Domain { get; set; }

        public int Port { get; set; }

        public String Email { get; set; }

        public String Password { get; set; }

        public String FromEmail { get; set; }

    }
}
