using Finbuckle.MultiTenant;
using Doctorla.Application.Exceptions;
using Doctorla.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Doctorla.Infrastructure.Notifications;

[Authorize]
public class NotificationHub : Hub, ITransientService
{
    private readonly ITenantInfo? currentTenant = null;
    private readonly ILogger<NotificationHub> logger = null;

    public NotificationHub(ITenantInfo? currentTenant, ILogger<NotificationHub> logger)
    {
        this.currentTenant = currentTenant;
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (currentTenant is null)
        {
            throw new UnauthorizedException("Authentication Failed.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"GroupTenant-{currentTenant.Id}");

        await base.OnConnectedAsync();

        logger.LogInformation("A client connected to NotificationHub: {connectionId}", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"GroupTenant-{currentTenant!.Id}");

        await base.OnDisconnectedAsync(exception);

        logger.LogInformation("A client disconnected from NotificationHub: {connectionId}", Context.ConnectionId);
    }
}