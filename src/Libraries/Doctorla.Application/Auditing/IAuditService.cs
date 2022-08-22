using Doctorla.Application.Common.Interfaces;

namespace Doctorla.Application.Auditing;

public interface IAuditService : ITransientService
{
    Task<List<AuditDto>> GetUserTrailsAsync(Guid userId);
}