namespace Doctorla.Application.Multitenancy;

public class GetAllTenantsRequest : IRequest<List<TenantDto>>
{
}

public class GetAllTenantsRequestHandler : IRequestHandler<GetAllTenantsRequest, List<TenantDto>>
{
    private readonly ITenantService tenantService = null;

    public GetAllTenantsRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

    public Task<List<TenantDto>> Handle(GetAllTenantsRequest request, CancellationToken cancellationToken) =>
        this.tenantService.GetAllAsync();
}