using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Models
{
    public class NextLevelDBSettings : INextLevelDBSettings
    {
        public string UsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
    }
    public interface INextLevelDBSettings
    {
        string UsersCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string Server { get; set; }
        int Port { get; set; }
    }
}
