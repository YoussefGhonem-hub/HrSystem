using System.Linq;
using HrSystem.Domain.Common;

namespace HrSystem.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Adds a reusable filter that excludes soft-deleted records.
    /// Keeps LINQ queries clean while still allowing composability with other filters.
    /// </summary>
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query)
        where T : BaseAuditableEntity
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.Where(entity => !entity.IsDeleted);
    }

    /// <summary>
    /// Convenience overload to apply the predicate conditionally (e.g., when dynamically building queries).
    /// </summary>
    public static IQueryable<T> WhereNotDeletedIf<T>(this IQueryable<T> query, bool condition)
        where T : BaseAuditableEntity
    {
        return condition ? query.WhereNotDeleted() : query;
    }
}
