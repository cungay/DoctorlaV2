using ServiceStack.OrmLite;

namespace Doctorla.Application.Common.Specification;

/*
public class EntitiesByBaseFilterSpec<T, TResult> : Specification<T, TResult>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}
*/

public class EntitiesByBaseFilterSpec<T, TResult> : SqlExpression<T>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) : base(SqlServer2019Dialect.Provider)
    {
        //Query.SearchBy(filter);
    }
}

/*
public class EntitiesByBaseFilterSpec<T> : Specification<T>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}
*/