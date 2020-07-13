using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.AuthDetails
{
    public interface IUserContext
    {
        Guid UserID { get; }
    }
}
