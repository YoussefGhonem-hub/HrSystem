using System.Linq;
using HrSystem.Domain.Common;
using HrSystem.Shared.CurrentUser;

namespace HrSystem.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies branch-level scope filtering. Use this on root query entities that need
    /// branch isolation. Not needed in global query filters (which only handle tenant + soft-delete)
    /// because global branch filters break Include() joins on navigation entities.
    /// </summary>
    public static IQueryable<T> ApplyBranchScope<T>(this IQueryable<T> query) where T : BaseEntity
    {
        if (CurrentUser.BypassScopeFilters || CurrentUser.IsSuperAdmin || CurrentUser.IsOrganizationAdmin)
            return query;

        var branchId = CurrentUser.BranchId;
        return query.Where(e => e.BranchId == branchId || e.BranchId == null);
    }

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
