namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;

public static class OffboardingTaskFilterExtensions
{
    public static IQueryable<Domain.Entities.Lifecycle.OffboardingTask> ApplyFilters(
        this IQueryable<Domain.Entities.Lifecycle.OffboardingTask> query,
        Guid? employeeId,
        bool? isCompleted,
        string? category,
        DateTime? dueDateFrom,
        DateTime? dueDateTo)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == employeeId.Value);
        }

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(t => t.Category.ToLower().Contains(category.ToLower()));
        }

        if (dueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueDateFrom.Value.Date);
        }

        if (dueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueDateTo.Value.Date);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.OffboardingTask> ApplySorting(
        this IQueryable<Domain.Entities.Lifecycle.OffboardingTask> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "sequence" => isDescending ? query.OrderByDescending(t => t.Sequence) : query.OrderBy(t => t.Sequence),
            "duedate" => isDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "employee" => isDescending ? query.OrderByDescending(t => t.Employee.FullNameEn) : query.OrderBy(t => t.Employee.FullNameEn),
            "category" => isDescending ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
            _ => query.OrderBy(t => t.Sequence)
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.OffboardingTask> ApplyPaging(
        this IQueryable<Domain.Entities.Lifecycle.OffboardingTask> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
