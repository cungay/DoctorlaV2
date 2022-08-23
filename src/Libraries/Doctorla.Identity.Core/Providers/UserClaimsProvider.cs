﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Doctorla.Identity.Core.Models;
using Doctorla.Identity.Core.Stores;
using Dapper;

namespace Doctorla.Identity.Core.Providers
{
    internal class UserClaimsProvider
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public UserClaimsProvider(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user) {
             var command = "SELECT * " +
                                   $"FROM [{_databaseConnectionFactory.DbSchema}].[AspNetUserClaims] " +
                                   "WHERE UserId = @UserId;";

             await using var sqlConnection = await _databaseConnectionFactory.CreateConnectionAsync();
             return (
                     await sqlConnection.QueryAsync<UserClaim>(command, new { UserId = user.Id })
                 )
                 .Select(e => new Claim(e.ClaimType, e.ClaimValue))
                 .ToList();
        }
    }
}
