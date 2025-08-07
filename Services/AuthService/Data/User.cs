using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Data
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }

        public ICollection<UserRoles> userRoles;
        public ICollection<UserClaim> userClaims;
        public ICollection<UserToken> userTokens;

        public ICollection<UserLogin> userLogins;
    }
}