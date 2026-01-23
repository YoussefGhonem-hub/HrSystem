namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;

public static class PerformanceReviewFilterExtensions
{
    public static IQueryable<Domain.Entities.Performance.PerformanceReview> ApplyFilters(
        this IQueryable<Domain.Entities.Performance.PerformanceReview> query,
        Guid? employeeId,
        Guid? reviewerId,
        string? reviewType,
        string? status,
        DateTime? reviewDateFrom,
        DateTime? reviewDateTo,
        bool? employeeAcknowledged)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }

        if (reviewerId.HasValue)
        {
            query = query.Where(r => r.ReviewerId == reviewerId.Value);
        }

        if (!string.IsNullOrEmpty(reviewType))
        {
            query = query.Where(r => r.ReviewType != null && r.ReviewType.NameEn.ToLower().Contains(reviewType.ToLower()));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status != null && r.Status.NameEn.ToLower() == status.ToLower());
        }

        if (reviewDateFrom.HasValue)
        {
            query = query.Where(r => r.ReviewDate >= reviewDateFrom.Value.Date);
        }

        if (reviewDateTo.HasValue)
        {
            query = query.Where(r => r.ReviewDate <= reviewDateTo.Value.Date);
        }

        if (employeeAcknowledged.HasValue)
        {
            query = query.Where(r => r.EmployeeAcknowledged == employeeAcknowledged.Value);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Performance.PerformanceReview> ApplySorting(
        this IQueryable<Domain.Entities.Performance.PerformanceReview> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "reviewdate" => isDescending ? query.OrderByDescending(r => r.ReviewDate) : query.OrderBy(r => r.ReviewDate),
            "employee" => isDescending ? query.OrderByDescending(r => r.Employee.FullNameEn) : query.OrderBy(r => r.Employee.FullNameEn),
            "reviewer" => isDescending ? query.OrderByDescending(r => r.Reviewer.FullNameEn) : query.OrderBy(r => r.Reviewer.FullNameEn),
            "rating" => isDescending ? query.OrderByDescending(r => r.OverallRating) : query.OrderBy(r => r.OverallRating),
            "status" => isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            _ => query.OrderByDescending(r => r.ReviewDate)
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Performance.PerformanceReview> ApplyPaging(
        this IQueryable<Domain.Entities.Performance.PerformanceReview> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
