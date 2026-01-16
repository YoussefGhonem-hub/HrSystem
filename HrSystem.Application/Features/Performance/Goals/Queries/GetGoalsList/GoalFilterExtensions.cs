namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;

public static class GoalFilterExtensions
{
    public static IQueryable<Domain.Entities.Performance.Goal> ApplyFilters(
        this IQueryable<Domain.Entities.Performance.Goal> query,
        Guid? employeeId,
        string? status,
        string? priority,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        DateTime? targetDateFrom,
        DateTime? targetDateTo)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(g => g.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusLower = status.ToLower();
            query = query.Where(g => g.Status.NameEn.ToLower() == statusLower || g.Status.NameAr.ToLower() == statusLower);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            var priorityLower = priority.ToLower();
            query = query.Where(g => g.Priority.NameEn.ToLower() == priorityLower || g.Priority.NameAr.ToLower() == priorityLower);
        }

        if (startDateFrom.HasValue)
        {
            query = query.Where(g => g.StartDate >= startDateFrom.Value.Date);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(g => g.StartDate <= startDateTo.Value.Date);
        }

        if (targetDateFrom.HasValue)
        {
            query = query.Where(g => g.TargetDate >= targetDateFrom.Value.Date);
        }

        if (targetDateTo.HasValue)
        {
            query = query.Where(g => g.TargetDate <= targetDateTo.Value.Date);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Performance.Goal> ApplySorting(
        this IQueryable<Domain.Entities.Performance.Goal> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(g => g.TitleEn) : query.OrderBy(g => g.TitleEn),
            "employee" => isDescending ? query.OrderByDescending(g => g.Employee.FullNameEn) : query.OrderBy(g => g.Employee.FullNameEn),
            "startdate" => isDescending ? query.OrderByDescending(g => g.StartDate) : query.OrderBy(g => g.StartDate),
            "targetdate" => isDescending ? query.OrderByDescending(g => g.TargetDate) : query.OrderBy(g => g.TargetDate),
            "status" => isDescending ? query.OrderByDescending(g => g.Status) : query.OrderBy(g => g.Status),
            "priority" => isDescending ? query.OrderByDescending(g => g.Priority) : query.OrderBy(g => g.Priority),
            "progress" => isDescending ? query.OrderByDescending(g => g.Progress) : query.OrderBy(g => g.Progress),
            _ => query.OrderByDescending(g => g.StartDate)
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Performance.Goal> ApplyPaging(
        this IQueryable<Domain.Entities.Performance.Goal> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
