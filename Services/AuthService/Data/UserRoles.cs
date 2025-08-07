using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Data
{
    public class UserRoles : IdentityUserRole<int>
    {
        public User user;
        public Role role; 
    }
}