using System.Collections.Generic;
using System.Security.Claims;
using Doctorla.Identity.Core.Stores;
using Microsoft.AspNetCore.Identity;

namespace Doctorla.Identity.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int UserType { get; set; } 
        public bool IsActive { get; set; } = true;

        internal List<Claim> Claims { get; set; }
        internal List<UserRole> Roles { get; set; }
        internal List<UserLoginInfo> Logins { get; set; }
        internal List<UserToken> Tokens { get; set; }
    }
}
