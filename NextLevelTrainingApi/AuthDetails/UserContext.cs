using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.AuthDetails
{
    public class UserContext : IUserContext
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserClaimValue(string claimType)
        {
            string value = _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == claimType)
                   .Select(c => c.Value).SingleOrDefault();
            return value;
        }

        public Guid UserID
        {
            get { return Guid.Parse(GetUserClaimValue("UserID")); }
        }
    }
}
