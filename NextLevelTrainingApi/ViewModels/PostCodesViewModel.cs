using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class PostCodesResponseModel
    {
        public List<PostCodesResult> Result { get; set; }
    }

    public class PostCodesResult
    {
        public string Postcode { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
    }
}
