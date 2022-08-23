using System.Security.Claims;
using Doctorla.Identity.Core.Stores;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Doctorla.Identity.Core.Providers
{
    internal class RoleClaimsProvider
    {
        private readonly IDbConnectionFactory databaseConnectionFactory = null;
        private readonly DBProviderOptions options = null;

        public RoleClaimsProvider(IDbConnectionFactory databaseConnectionFactory, DBProviderOptions options)
        {
            this.databaseConnectionFactory = databaseConnectionFactory;
            this.options = options;
        }

        public async Task<IList<Claim>> GetClaimsAsync(string roleId)
        {
            using (var db = databaseConnectionFactory.Open())
            {

            }

            var command = "SELECT * " +
                                   $"FROM [{_databaseConnectionFactory.}].[AspNetRoleClaims] " +
                                   "WHERE RoleId = @RoleId;";

            IEnumerable<RoleClaim> roleClaims = new List<RoleClaim>();

            await using var sqlConnection = await _databaseConnectionFactory.CreateConnectionAsync();
            return (
                    await sqlConnection.QueryAsync<RoleClaim>(command, new { RoleId = roleId })
                )
                .Select(x => new Claim(x.ClaimType, x.ClaimValue))
                .ToList();
        }
    }
}
