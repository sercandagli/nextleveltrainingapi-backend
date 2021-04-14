using System;
namespace NextLevelTrainingApi.ViewModels
{
    public class PaymentIntentViewModel
    {
        public string Id { get; set; }
        public long Amount { get; set; }
        public string ClientSecret { get; set; }
        public string Status { get; set; }
    }
}
