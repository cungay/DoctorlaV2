using Finbuckle.MultiTenant;
using Doctorla.Application.Common.Events;
using Doctorla.Application.Common.Exceptions;
using Doctorla.Application.Common.Interfaces;
using Doctorla.Application.Identity.Roles;
using Doctorla.Domain.Identity;
using Doctorla.Infrastructure.Persistence.Context;
using Doctorla.Shared.Authorization;
using Doctorla.Shared.Multitenancy;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Doctorla.Infrastructure.Identity;

internal class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> roleManager = null;
    private readonly UserManager<ApplicationUser> userManager = null;
    private readonly ApplicationDbContext db = null;
    private readonly IStringLocalizer localizer = null;
    private readonly ICurrentUser  currentUser = null;
    private readonly ITenantInfo currentTenant = null;
    private readonly IEventPublisher events = null;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IStringLocalizer<RoleService> localizer,
        ICurrentUser currentUser,
        ITenantInfo currentTenant,
        IEventPublisher events)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.db = db;
        this.localizer = localizer;
        this.currentUser = currentUser;
        this.currentTenant = currentTenant;
        this.events = events;
    }

    public async Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken) =>
        (await roleManager.Roles.ToListAsync(cancellationToken))
            .Adapt<List<RoleDto>>();

    public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        await roleManager.Roles.CountAsync(cancellationToken);

    public async Task<bool> ExistsAsync(string roleName, string? excludeId) =>
        await roleManager.FindByNameAsync(roleName)
            is ApplicationRole existingRole
            && existingRole.Id != excludeId;

    public async Task<RoleDto> GetByIdAsync(string id) =>
        await db.Roles.SingleOrDefaultAsync(x => x.Id == id) is { } role
            ? role.Adapt<RoleDto>()
            : throw new NotFoundException(localizer["Role Not Found"]);

    public async Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = await GetByIdAsync(roleId);

        role.Permissions = await db.RoleClaims
            .Where(c => c.RoleId == roleId && c.ClaimType == DocClaims.Permission)
            .Select(c => c.ClaimValue)
            .ToListAsync(cancellationToken);

        return role;
    }

    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            // Create a new role.
            var role = new ApplicationRole(request.Name, request.Description);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException(localizer["Register role failed"], result.GetErrors(localizer));
            }

            await events.PublishAsync(new ApplicationRoleCreatedEvent(role.Id, role.Name));

            return string.Format(localizer["Role {0} Created."], request.Name);
        }
        else
        {
            // Update an existing role.
            var role = await roleManager.FindByIdAsync(request.Id);

            _ = role ?? throw new NotFoundException(localizer["Role Not Found"]);

            if (DocRoles.IsDefault(role.Name))
            {
                throw new ConflictException(string.Format(localizer["Not allowed to modify {0} Role."], role.Name));
            }

            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
            role.Description = request.Description;
            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException(localizer["Update role failed"], result.GetErrors(localizer));
            }

            await events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name));

            return string.Format(localizer["Role {0} Updated."], role.Name);
        }
    }

    public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId);
        _ = role ?? throw new NotFoundException(localizer["Role Not Found"]);
        if (role.Name == DocRoles.Admin)
        {
            throw new ConflictException(localizer["Not allowed to modify Permissions for this Role."]);
        }

        if (currentTenant.Id != MultitenancyConstants.Root.Id)
        {
            // Remove Root Permissions if the Role is not created for Root Tenant.
            request.Permissions.RemoveAll(u => u.StartsWith("Permissions.Root."));
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);

        // Remove permissions that were previously selected
        foreach (var claim in currentClaims.Where(c => !request.Permissions.Any(p => p == c.Value)))
        {
            var removeResult = await roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
            {
                throw new InternalServerException(localizer["Update permissions failed."], removeResult.GetErrors(localizer));
            }
        }

        // Add all permissions that were not previously selected
        foreach (string permission in request.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
        {
            if (!string.IsNullOrEmpty(permission))
            {
                db.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = DocClaims.Permission,
                    ClaimValue = permission,
                    CreatedBy = currentUser.GetUserId().ToString()
                });
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        await events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name, true));

        return localizer["Permissions Updated."];
    }

    public async Task<string> DeleteAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        _ = role ?? throw new NotFoundException(localizer["Role Not Found"]);

        if (DocRoles.IsDefault(role.Name))
        {
            throw new ConflictException(string.Format(localizer["Not allowed to delete {0} Role."], role.Name));
        }

        if ((await userManager.GetUsersInRoleAsync(role.Name)).Count > 0)
        {
            throw new ConflictException(string.Format(localizer["Not allowed to delete {0} Role as it is being used."], role.Name));
        }

        await roleManager.DeleteAsync(role);

        await events.PublishAsync(new ApplicationRoleDeletedEvent(role.Id, role.Name));

        return string.Format(localizer["Role {0} Deleted."], role.Name);
    }
}