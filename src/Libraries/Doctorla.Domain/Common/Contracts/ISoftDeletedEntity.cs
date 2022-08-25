namespace Doctorla.Domain.Common.Contracts;

public interface ISoftDeletedEntity
{
    bool Deleted
    {
        get
        {
            var deleted = (DeletedOn.HasValue && DeletedOn.Value > DateTime.MinValue) && (DeletedBy.HasValue && DeletedBy.Value != Guid.Empty);
            return deleted;
        }
    }

    DateTime? DeletedOn { get; set; }

    Guid? DeletedBy { get; set; }
}