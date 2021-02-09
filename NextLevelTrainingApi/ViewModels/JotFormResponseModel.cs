using System;
using System.Collections.Generic;

namespace NextLevelTrainingApi.ViewModels
{
    public class JotFormResponseModel
    {
        public int ResponseCode { get; set; }
        public List<ContentModel> Content { get; set; }
    }

    public class ContentModel
    {
        public string Id { get; set; }
        public Dictionary<string, AnswerModel> Answers { get; set; }
    }

    public class AnswerModel
    {
        public string Name { get; set; }
        public dynamic Answer { get; set; }
        public string PrettyFormat { get; set; }
    }
}
