using Finbuckle.MultiTenant;
using Doctorla.Infrastructure.Auth;
using Doctorla.Infrastructure.Common;
using Doctorla.Infrastructure.Multitenancy;
using Doctorla.Shared.Multitenancy;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Doctorla.Infrastructure.BackgroundJobs;

public class DocJobActivator : JobActivator
{
    private readonly IServiceScopeFactory scopeFactory = null;

    public DocJobActivator(IServiceScopeFactory scopeFactory) =>
        this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

    public override JobActivatorScope BeginScope(PerformContext context) =>
        new Scope(context, scopeFactory.CreateScope());

    private class Scope : JobActivatorScope, IServiceProvider
    {
        private readonly PerformContext context = null;
        private readonly IServiceScope scope = null;

        public Scope(PerformContext context, IServiceScope scope)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));

            ReceiveParameters();
        }

        private void ReceiveParameters()
        {
            var tenantInfo = context.GetJobParameter<DocTenantInfo>(MultitenancyConstants.TenantIdName);
            if (tenantInfo is not null)
            {
                scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor>()
                    .MultiTenantContext = new MultiTenantContext<DocTenantInfo>
                    {
                        TenantInfo = tenantInfo
                    };
            }

            string userId = context.GetJobParameter<string>(QueryStringKeys.UserId);
            if (!string.IsNullOrEmpty(userId))
            {
                scope.ServiceProvider.GetRequiredService<ICurrentUserInitializer>()
                    .SetCurrentUserId(userId);
            }
        }

        public override object Resolve(Type type) =>
            ActivatorUtilities.GetServiceOrCreateInstance(this, type);

        object? IServiceProvider.GetService(Type serviceType) =>
            serviceType == typeof(PerformContext)
                ? context
                : scope.ServiceProvider.GetService(serviceType);
    }
}