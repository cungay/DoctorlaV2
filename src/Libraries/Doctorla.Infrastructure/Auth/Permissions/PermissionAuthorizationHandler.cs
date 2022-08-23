using System.Security.Claims;
using Doctorla.Application.Identity.Users;
using Microsoft.AspNetCore.Authorization;

namespace Doctorla.Infrastructure.Auth.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserService userService = null;

    public PermissionAuthorizationHandler(IUserService userService) =>
        this.userService = userService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User?.GetUserId() is { } userId &&
            await userService.HasPermissionAsync(userId, requirement.Permission)) {
            context.Succeed(requirement);
        }
    }
}