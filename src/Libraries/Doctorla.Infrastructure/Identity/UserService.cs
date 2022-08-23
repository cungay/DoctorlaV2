using Finbuckle.MultiTenant;
using Doctorla.Application.Common.Caching;
using Doctorla.Application.Common.Events;
using Doctorla.Application.Common.Exceptions;
using Doctorla.Application.Common.FileStorage;
using Doctorla.Application.Common.Interfaces;
using Doctorla.Application.Common.Mailing;
using Doctorla.Application.Common.Models;
using Doctorla.Application.Identity.Users;
using Doctorla.Domain.Identity;
using Doctorla.Infrastructure.Auth;
using Doctorla.Infrastructure.Persistence.Context;
using Doctorla.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Doctorla.Infrastructure.Identity;

internal partial class UserService : IUserService
{
    private readonly SignInManager<ApplicationUser> signInManager = null;
    private readonly UserManager<ApplicationUser> userManager = null;
    private readonly RoleManager<ApplicationRole> roleManager = null;
    private readonly ApplicationDbContext db = null;
    private readonly IStringLocalizer localizer = null;
    private readonly IJobService jobService = null;
    private readonly IMailService mailService = null;
    private readonly SecuritySettings securitySettings = null;
    private readonly IEmailTemplateService templateService = null;
    private readonly IFileStorageService fileStorage = null;
    private readonly IEventPublisher events = null;
    private readonly ICacheService cache = null;
    private readonly ICacheKeyService cacheKeys = null;
    private readonly ITenantInfo currentTenant = null;

    public UserService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext db,
        IStringLocalizer<UserService> localizer,
        IJobService jobService,
        IMailService mailService,
        IEmailTemplateService templateService,
        IFileStorageService fileStorage,
        IEventPublisher events,
        ICacheService cache,
        ICacheKeyService cacheKeys,
        ITenantInfo currentTenant,
        IOptions<SecuritySettings> securitySettings)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.db = db;
        this.localizer = localizer;
        this.jobService = jobService;
        this.mailService = mailService;
        this.templateService = templateService;
        this.fileStorage = fileStorage;
        this.events = events;
        this.cache = cache;
        this.cacheKeys = cacheKeys;
        this.currentTenant = currentTenant;
        this.securitySettings = securitySettings.Value;
    }

    public async Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
    {
        //var spec = new EntitiesByPaginationFilterSpec<ApplicationUser>(filter);

        var users = await userManager.Users
            //.WithSpecification(spec)
            .ProjectToType<UserDetailsDto>()
            .ToListAsync(cancellationToken);
        int count = await userManager.Users
            .CountAsync(cancellationToken);

        return new PaginationResponse<UserDetailsDto>(users, count, filter.PageNumber, filter.PageSize);
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        EnsureValidTenant();
        return await userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
    {
        EnsureValidTenant();
        return await userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
    {
        EnsureValidTenant();
        return await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user && user.Id != exceptId;
    }

    private void EnsureValidTenant()
    {
        if (string.IsNullOrWhiteSpace(currentTenant?.Id))
        {
            throw new UnauthorizedException(localizer["Invalid Tenant."]);
        }
    }

    public async Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken) =>
        (await userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .Adapt<List<UserDetailsDto>>();

    public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        userManager.Users.AsNoTracking().CountAsync(cancellationToken);

    public async Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        _ = user ?? throw new NotFoundException(localizer["User Not Found."]);

        return user.Adapt<UserDetailsDto>();
    }

    public async Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);

        _ = user ?? throw new NotFoundException(localizer["User Not Found."]);

        bool isAdmin = await userManager.IsInRoleAsync(user, DocRoles.Admin);
        if (isAdmin)
        {
            throw new ConflictException(localizer["Administrators Profile's Status cannot be toggled"]);
        }

        user.IsActive = request.ActivateUser;

        await userManager.UpdateAsync(user);

        await events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));
    }
}