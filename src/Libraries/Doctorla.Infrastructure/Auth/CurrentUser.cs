using System.Security.Claims;
using Doctorla.Application.Interfaces;

namespace Doctorla.Infrastructure.Auth;

public class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? currentUser = null;

    public string? Name => currentUser?.Identity?.Name;

    private Guid currentUserId = Guid.Empty;

    public Guid GetUserId() =>
        IsAuthenticated()
            ? Guid.Parse(currentUser?.GetUserId() ?? Guid.Empty.ToString())
            : currentUserId;

    public string? GetUserEmail() =>
        IsAuthenticated()
            ? currentUser!.GetEmail()
            : string.Empty;

    public bool IsAuthenticated() =>
        currentUser?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) =>
        currentUser?.IsInRole(role) is true;

    public IEnumerable<Claim>? GetUserClaims() =>
        currentUser?.Claims;

    public string? GetTenant() =>
        IsAuthenticated() ? currentUser?.GetTenant() : string.Empty;

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (currentUser != null)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        currentUser = user;
    }

    public void SetCurrentUserId(string userId)
    {
        if (currentUserId != Guid.Empty)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            currentUserId = Guid.Parse(userId);
        }
    }
}