using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class ErrorViewModel
    {
        public Error errors { get; set; }
        public string title { get; set; }
        public int status { get; set; }
    }

    public class Error
    {
        public string[] error { get; set; }
    }
}
