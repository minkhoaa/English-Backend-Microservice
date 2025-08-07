using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Data
{
    public class UserClaim : IdentityUserClaim<int>
    {
        public User user; 
    }
}