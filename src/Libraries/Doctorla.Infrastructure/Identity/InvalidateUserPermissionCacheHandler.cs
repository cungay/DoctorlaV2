using Doctorla.Application.Common.Events;
using Doctorla.Application.Identity.Users;
using Doctorla.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Doctorla.Infrastructure.Identity;

internal class InvalidateUserPermissionCacheHandler :
    IEventNotificationHandler<ApplicationUserUpdatedEvent>,
    IEventNotificationHandler<ApplicationRoleUpdatedEvent>
{
    private readonly IUserService userService = null;
    private readonly UserManager<ApplicationUser> userManager = null;

    public InvalidateUserPermissionCacheHandler(IUserService userService, UserManager<ApplicationUser> userManager) =>
        (this.userService, this.userManager) = (userService, userManager);

    public async Task Handle(EventNotification<ApplicationUserUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        if (notification.Event.RolesUpdated)
        {
            await this.userService.InvalidatePermissionCacheAsync(notification.Event.UserId, cancellationToken);
        }
    }

    public async Task Handle(EventNotification<ApplicationRoleUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        if (notification.Event.PermissionsUpdated)
        {
            foreach (var user in await this.userManager.GetUsersInRoleAsync(notification.Event.RoleName))
            {
                await this.userService.InvalidatePermissionCacheAsync(user.Id, cancellationToken);
            }
        }
    }
}