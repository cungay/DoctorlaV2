using Doctorla.Infrastructure.Multitenancy;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(DocTenantInfo tenant, CancellationToken cancellationToken);
}