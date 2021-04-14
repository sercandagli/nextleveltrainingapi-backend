using System;
using System.ComponentModel.DataAnnotations;

namespace NextLevelTrainingApi.ViewModels
{
    public class PayWithStripeViewModel
    {
        [Required]
        public long Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        public string StatementDescriptor { get; set; }
    }
}
