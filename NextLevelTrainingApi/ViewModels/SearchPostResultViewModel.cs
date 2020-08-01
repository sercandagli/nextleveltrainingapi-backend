using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class SearchPostResultViewModel
    {
        public List<UserDataViewModel> Players { get; set; }
        public List<UserDataViewModel> Coaches { get; set; }
        public List<PostDataViewModel> Posts { get; set; }
    }

    public class SearchUserViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string PostCode { get; set; }
        public string Address { get; set; }

        public string EmailID { get; set; }

        public string MobileNo { get; set; }

        public string Role { get; set; }
        public string ProfileImage { get; set; }

    }
}
