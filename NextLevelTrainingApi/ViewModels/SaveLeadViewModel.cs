using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class SaveLeadViewModel
    {
        public string Experience { get; set; }
        public string Age { get; set; }
        public List<string >CoachingType { get; set; }
        public List<string> Days { get; set; }
        public List<string> CoachingTime { get; set; }
        public List<string> DaysOfWeek { get; set; }
        public string FullName { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string MaximumPrice { get; set; }

        public string Location { get; set; }
    }
}
