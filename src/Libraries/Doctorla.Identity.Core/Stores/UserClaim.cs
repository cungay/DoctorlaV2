﻿using ServiceStack.DataAnnotations;

namespace Doctorla.Identity.Core.Stores
{
    //[Schema()]
    internal class UserClaim
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
