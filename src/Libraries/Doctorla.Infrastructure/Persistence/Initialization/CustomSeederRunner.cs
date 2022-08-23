using Microsoft.Extensions.DependencyInjection;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] seeders = null;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}