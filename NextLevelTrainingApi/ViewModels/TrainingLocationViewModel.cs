using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class TrainingLocationViewModel
    {
        public Guid UserId { get; set; }
        public Guid TrainingLocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime LocationAddress { get; set; }
    }
}
