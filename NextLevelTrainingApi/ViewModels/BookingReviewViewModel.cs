using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class BookingReviewViewModel
    {
        public Guid? Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid BookingId { get; set; }
        public int Rating { get; set; }
        public string Feedback { get; set; }
    }
}
