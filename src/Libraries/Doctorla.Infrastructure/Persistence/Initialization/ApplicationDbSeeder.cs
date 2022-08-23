using Doctorla.Infrastructure.Identity;
using Doctorla.Infrastructure.Multitenancy;
using Doctorla.Infrastructure.Persistence.Context;
using Doctorla.Shared.Authorization;
using Doctorla.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly FSHTenantInfo currentTenant = null;
    private readonly RoleManager<ApplicationRole> roleManager = null;
    private readonly UserManager<ApplicationUser> userManager = null;
    private readonly CustomSeederRunner seederRunner = null;
    private readonly ILogger<ApplicationDbSeeder> logger = null;

    public ApplicationDbSeeder(FSHTenantInfo currentTenant, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        this.currentTenant = currentTenant;
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.seederRunner = seederRunner;
        this.logger = logger;
    }

    public async Task SeedDatabaseAsync(IDbConnectionFactory dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(IDbConnectionFactory dbContext)
    {
        foreach (string roleName in FSHRoles.DefaultRoles)
        {
            if (await roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                logger.LogInformation("Seeding {role} Role for '{tenantId}' Tenant.", roleName, currentTenant.Id);
                role = new ApplicationRole(roleName, $"{roleName} Role for {currentTenant.Id} Tenant");
                await roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == FSHRoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, FSHPermissions.Basic, role);
            }
            else if (roleName == FSHRoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, FSHPermissions.Admin, role);

                if (currentTenant.Id == MultitenancyConstants.Root.Id)
                {
                    await AssignPermissionsToRoleAsync(dbContext, FSHPermissions.Root, role);
                }
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(IDbConnectionFactory dbContext, IReadOnlyList<FSHPermission> permissions, ApplicationRole role)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == FSHClaims.Permission && c.Value == permission.Name))
            {
                logger.LogInformation("Seeding {role} Permission '{permission}' for '{tenantId}' Tenant.", role.Name, permission.Name, currentTenant.Id);
                /*
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = FSHClaims.Permission,
                    ClaimValue = permission.Name,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync();
                */
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (string.IsNullOrWhiteSpace(currentTenant.Id) || string.IsNullOrWhiteSpace(currentTenant.AdminEmail))
        {
            return;
        }

        if (await userManager.Users.FirstOrDefaultAsync(u => u.Email == currentTenant.AdminEmail)
            is not ApplicationUser adminUser)
        {
            string adminUserName = $"{currentTenant.Id.Trim()}.{FSHRoles.Admin}".ToLowerInvariant();
            adminUser = new ApplicationUser
            {
                FirstName = currentTenant.Id.Trim().ToLowerInvariant(),
                LastName = FSHRoles.Admin,
                Email = currentTenant.AdminEmail,
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = currentTenant.AdminEmail?.ToUpperInvariant(),
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                IsActive = true
            };

            logger.LogInformation("Seeding Default Admin User for '{tenantId}' Tenant.", currentTenant.Id);
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, MultitenancyConstants.DefaultPassword);
            await userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await userManager.IsInRoleAsync(adminUser, FSHRoles.Admin))
        {
            logger.LogInformation("Assigning Admin Role to Admin User for '{tenantId}' Tenant.", currentTenant.Id);
            await userManager.AddToRoleAsync(adminUser, FSHRoles.Admin);
        }
    }
}