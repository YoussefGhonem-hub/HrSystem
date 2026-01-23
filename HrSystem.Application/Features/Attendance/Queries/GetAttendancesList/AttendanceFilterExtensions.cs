namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public static class AttendanceFilterExtensions
{
    public static IQueryable<Domain.Entities.Attendance.Attendance> ApplyFilters(
        this IQueryable<Domain.Entities.Attendance.Attendance> query,
        Guid? employeeId,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? statusId,
        bool? isLate,
        bool? isOvertime)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Date <= toDate.Value.Date);
        }

        if (statusId.HasValue)
        {
            query = query.Where(a => a.StatusId == statusId.Value);
        }

        if (isLate.HasValue)
        {
            query = query.Where(a => a.IsLate == isLate.Value);
        }

        if (isOvertime.HasValue)
        {
            query = query.Where(a => a.IsOvertime == isOvertime.Value);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Attendance.Attendance> ApplySorting(
        this IQueryable<Domain.Entities.Attendance.Attendance> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date),
            "employee" => isDescending ? query.OrderByDescending(a => a.Employee.FullNameEn) : query.OrderBy(a => a.Employee.FullNameEn),
            "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "workedhours" => isDescending ? query.OrderByDescending(a => a.WorkedHours) : query.OrderBy(a => a.WorkedHours),
            _ => query.OrderByDescending(a => a.Date) // Default sort by date descending
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Attendance.Attendance> ApplyPaging(
        this IQueryable<Domain.Entities.Attendance.Attendance> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
