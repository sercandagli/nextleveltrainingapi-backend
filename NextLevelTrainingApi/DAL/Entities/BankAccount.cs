using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class BankAccount
    {
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
    }
}
