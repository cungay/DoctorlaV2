namespace Doctorla.Domain.Common.Contracts;

public interface ISoftDeletedEntity
{
    DateTime? DeletedOn { get; set; }
    Guid? DeletedBy { get; set; }
}