namespace Doctorla.Application.Multitenancy;

public class CreateTenantRequest : IRequest<string>
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;
    
    public string? ConnectionString { get; set; }
    
    public string AdminEmail { get; set; } = default!;
    
    public string? Issuer { get; set; }
}

public class CreateTenantRequestHandler : IRequestHandler<CreateTenantRequest, string>
{
    private readonly ITenantService tenantService = null;

    public CreateTenantRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

    public Task<string> Handle(CreateTenantRequest request, CancellationToken cancellationToken) =>
        this.tenantService.CreateAsync(request, cancellationToken);
}