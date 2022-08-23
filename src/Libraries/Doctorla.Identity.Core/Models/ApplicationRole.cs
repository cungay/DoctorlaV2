using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Doctorla.Identity.Core.Models
{
    public class ApplicationRole : IdentityRole
    {
        internal List<Claim> Claims { get; set; }
    }
}
