namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;

public static class PolicyAcknowledgmentFilterExtensions
{
    public static IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> ApplyFilters(
        this IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> query,
        Guid? employeeId,
        string? policyName,
        bool? isAcknowledged,
        DateTime? acknowledgedDateFrom,
        DateTime? acknowledgedDateTo)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(policyName))
        {
            query = query.Where(p => p.PolicyName.ToLower().Contains(policyName.ToLower()));
        }

        if (isAcknowledged.HasValue)
        {
            query = query.Where(p => p.IsAcknowledged == isAcknowledged.Value);
        }

        if (acknowledgedDateFrom.HasValue)
        {
            query = query.Where(p => p.AcknowledgedDate >= acknowledgedDateFrom.Value.Date);
        }

        if (acknowledgedDateTo.HasValue)
        {
            query = query.Where(p => p.AcknowledgedDate <= acknowledgedDateTo.Value.Date);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> ApplySorting(
        this IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "policyname" => isDescending ? query.OrderByDescending(p => p.PolicyName) : query.OrderBy(p => p.PolicyName),
            "acknowledgeddate" => isDescending ? query.OrderByDescending(p => p.AcknowledgedDate) : query.OrderBy(p => p.AcknowledgedDate),
            "employee" => isDescending ? query.OrderByDescending(p => p.Employee.FullNameEn) : query.OrderBy(p => p.Employee.FullNameEn),
            "version" => isDescending ? query.OrderByDescending(p => p.PolicyVersion) : query.OrderBy(p => p.PolicyVersion),
            _ => query.OrderByDescending(p => p.AcknowledgedDate)
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> ApplyPaging(
        this IQueryable<Domain.Entities.Lifecycle.PolicyAcknowledgment> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
