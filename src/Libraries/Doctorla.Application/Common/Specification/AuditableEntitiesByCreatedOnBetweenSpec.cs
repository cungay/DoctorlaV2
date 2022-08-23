using ServiceStack.OrmLite;

namespace Doctorla.Application.Common.Specification;

public class AuditableEntitiesByCreatedOnBetweenSpec<T> : SqlExpression<T>
    where T : AuditableEntity
{
    public AuditableEntitiesByCreatedOnBetweenSpec(DateTime from, DateTime until) : base(dialectProvider: SqlServer2019Dialect.Provider)
    {
        this.Where<T>(e => e.CreatedOn >= from && e.CreatedOn <= until);
        //Query.Where(e => e.CreatedOn >= from && e.CreatedOn <= until);
    }
}