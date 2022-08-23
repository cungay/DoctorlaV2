using System.Security.Claims;
using Doctorla.Application.Common.Interfaces;

namespace Doctorla.Infrastructure.Auth;

public class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? user = null;

    public string? Name => user?.Identity?.Name;

    private Guid cachedUserId = Guid.Empty;

    public Guid GetUserId() =>
        IsAuthenticated()
            ? Guid.Parse(user?.GetUserId() ?? Guid.Empty.ToString())
            : cachedUserId;

    public string? GetUserEmail() =>
        IsAuthenticated()
            ? user!.GetEmail()
            : string.Empty;

    public bool IsAuthenticated() =>
        user?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) =>
        user?.IsInRole(role) is true;

    public IEnumerable<Claim>? GetUserClaims() =>
        user?.Claims;

    public string? GetTenant() =>
        IsAuthenticated() ? user?.GetTenant() : string.Empty;

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (user != null)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        this.user = user;
    }

    public void SetCurrentUserId(string userId)
    {
        if (cachedUserId != Guid.Empty)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            cachedUserId = Guid.Parse(userId);
        }
    }
}