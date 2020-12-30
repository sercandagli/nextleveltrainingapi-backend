using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class FileInputModel
    {
        public IFormFile File { get; set; }
        public string Type { get; set; }
        public Guid Id { get; set; }
    }
}
