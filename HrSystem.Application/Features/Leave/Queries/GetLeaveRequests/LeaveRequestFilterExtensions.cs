using HrSystem.Domain.Entities.Leave;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;

namespace HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;

public static class LeaveRequestFilterExtensions
{
    public static IQueryable<LeaveRequest> ApplyRoleBasedFilters(
        this IQueryable<LeaveRequest> query,
        Guid currentEmployeeId,
        bool isHRManager,
        bool isDepartmentManager,
        bool isEmployee)
    {
        if (isEmployee)
        {
            // Employee: Get all their own leave requests
            query = query.Where(lr => lr.EmployeeId == currentEmployeeId);
        }
        else if (isDepartmentManager)
        {
            // Department Manager: Get requests pending approval from direct reports
            query = query.Where(lr =>
                lr.LeaveStatusId == LeaveStatusIds.Pending &&
                lr.Employee.DirectManagerId == currentEmployeeId);
        }
        else if (isHRManager)
        {
            // HR Manager: Get requests that are manager approved and waiting for HR approval
            query = query.Where(lr => lr.LeaveStatusId == LeaveStatusIds.ManagerApproved);
        }

        return query;
    }

    public static IQueryable<LeaveRequest> ApplyStatusFilter(
        this IQueryable<LeaveRequest> query,
        Guid? statusId)
    {
        if (statusId.HasValue)
        {
            query = query.Where(lr => lr.LeaveStatusId == statusId.Value);
        }

        return query;
    }

    public static IQueryable<LeaveRequest> ApplyLeaveTypeFilter(
        this IQueryable<LeaveRequest> query,
        Guid? leaveTypeId)
    {
        if (leaveTypeId.HasValue)
        {
            query = query.Where(lr => lr.LeaveTypeId == leaveTypeId.Value);
        }

        return query;
    }

    public static IQueryable<LeaveRequest> ApplyDateRangeFilter(
        this IQueryable<LeaveRequest> query,
        DateTime? startDateFrom,
        DateTime? startDateTo)
    {
        if (startDateFrom.HasValue)
        {
            query = query.Where(lr => lr.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(lr => lr.StartDate <= startDateTo.Value);
        }

        return query;
    }

    public static IQueryable<LeaveRequest> ApplyEmployeeFilter(
        this IQueryable<LeaveRequest> query,
        Guid? employeeId)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);
        }

        return query;
    }

    public static IQueryable<LeaveRequest> ApplySorting(
        this IQueryable<LeaveRequest> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderBy(lr => lr.StartDate);
        }

        query = sortBy.ToLower() switch
        {
            "startdate" => sortDescending ? query.OrderByDescending(lr => lr.StartDate) : query.OrderBy(lr => lr.StartDate),
            "enddate" => sortDescending ? query.OrderByDescending(lr => lr.EndDate) : query.OrderBy(lr => lr.EndDate),
            "status" => sortDescending ? query.OrderByDescending(lr => lr.LeaveStatus.DisplayOrder) : query.OrderBy(lr => lr.LeaveStatus.DisplayOrder),
            "leavetype" => sortDescending ? query.OrderByDescending(lr => lr.LeaveType.DisplayOrder) : query.OrderBy(lr => lr.LeaveType.DisplayOrder),
            "totaldays" => sortDescending ? query.OrderByDescending(lr => lr.TotalDays) : query.OrderBy(lr => lr.TotalDays),
            "createddate" => sortDescending ? query.OrderByDescending(lr => lr.CreatedDate) : query.OrderBy(lr => lr.CreatedDate),
            _ => query.OrderBy(lr => lr.StartDate)
        };

        return query;
    }
}
