using Finbuckle.MultiTenant;
using Doctorla.Application.Exceptions;
using Doctorla.Application.Persistence;
using Doctorla.Application.Multitenancy;
using Doctorla.Infrastructure.Persistence;
using Doctorla.Infrastructure.Persistence.Initialization;
using Mapster;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Doctorla.Infrastructure.Multitenancy;

internal class TenantService : ITenantService
{
    private readonly IMultiTenantStore<DocTenantInfo> tenantStore = null;
    private readonly IConnectionStringSecurer csSecurer = null;
    private readonly IDatabaseInitializer dbInitializer = null;
    private readonly IStringLocalizer localizer = null;
    private readonly DatabaseSettings dbSettings = null;

    public TenantService(
        IMultiTenantStore<DocTenantInfo> tenantStore,
        IConnectionStringSecurer csSecurer,
        IDatabaseInitializer dbInitializer,
        IStringLocalizer<TenantService> localizer,
        IOptions<DatabaseSettings> dbSettings)
    {
        this.tenantStore = tenantStore;
        this.csSecurer = csSecurer;
        this.dbInitializer = dbInitializer;
        this.localizer = localizer;
        this.dbSettings = dbSettings.Value;
    }

    public async Task<List<TenantDto>> GetAllAsync()
    {
        var tenants = (await tenantStore.GetAllAsync()).Adapt<List<TenantDto>>();
        tenants.ForEach(t => t.ConnectionString = csSecurer.MakeSecure(t.ConnectionString));
        return tenants;
    }

    public async Task<bool> ExistsWithIdAsync(string id) =>
        await tenantStore.TryGetAsync(id) is not null;

    public async Task<bool> ExistsWithNameAsync(string name) =>
        (await tenantStore.GetAllAsync()).Any(t => t.Name == name);

    public async Task<TenantDto> GetByIdAsync(string id) =>
        (await GetTenantInfoAsync(id))
            .Adapt<TenantDto>();

    public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        if (request.ConnectionString?.Trim() == dbSettings.ConnectionString.Trim()) request.ConnectionString = string.Empty;

        var tenant = new DocTenantInfo(request.Id, request.Name, request.ConnectionString, request.AdminEmail, request.Issuer);
        await tenantStore.TryAddAsync(tenant);

        // TODO: run this in a hangfire job? will then have to send mail when it's ready or not
        try
        {
            await dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
        catch
        {
            await tenantStore.TryRemoveAsync(request.Id);
            throw;
        }

        return tenant.Id;
    }

    public async Task<string> ActivateAsync(string id)
    {
        var tenant = await GetTenantInfoAsync(id);

        if (tenant.IsActive)
        {
            throw new ConflictException(localizer["Tenant is already Activated."]);
        }

        tenant.Activate();

        await tenantStore.TryUpdateAsync(tenant);

        return localizer["Tenant {0} is now Activated.", id];
    }

    public async Task<string> DeactivateAsync(string id)
    {
        var tenant = await GetTenantInfoAsync(id);

        if (!tenant.IsActive)
        {
            throw new ConflictException(localizer["Tenant is already Deactivated."]);
        }

        tenant.Deactivate();

        await tenantStore.TryUpdateAsync(tenant);

        return localizer[$"Tenant {0} is now Deactivated.", id];
    }

    public async Task<string> UpdateSubscription(string id, DateTime extendedExpiryDate)
    {
        var tenant = await GetTenantInfoAsync(id);

        tenant.SetValidity(extendedExpiryDate);

        await tenantStore.TryUpdateAsync(tenant);

        return localizer[$"Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto];
    }

    private async Task<DocTenantInfo> GetTenantInfoAsync(string id) =>
        await tenantStore.TryGetAsync(id)
            ?? throw new NotFoundException(localizer["{0} {1} Not Found.", typeof(DocTenantInfo).Name, id]);
}