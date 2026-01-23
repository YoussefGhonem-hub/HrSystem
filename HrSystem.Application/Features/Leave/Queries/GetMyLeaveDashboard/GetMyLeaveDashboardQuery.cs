using ErrorOr;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveDashboard;

/// <summary>
/// Query to get leave dashboard data for the currently logged-in user
/// </summary>
public record GetMyLeaveDashboardQuery(int? Year = null, int HistoryCount = 5)
    : IRequest<ErrorOr<GenericResponse<MyLeaveDashboardDto>>>;

public class GetMyLeaveDashboardQueryHandler : IRequestHandler<GetMyLeaveDashboardQuery, ErrorOr<GenericResponse<MyLeaveDashboardDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyLeaveDashboardQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyLeaveDashboardDto>>> Handle(
        GetMyLeaveDashboardQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("LeaveDashboard.Unauthorized", "Current user is not linked to an employee");
        }

        var year = request.Year ?? DateTime.UtcNow.Year;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeavePolicy)
                .ThenInclude(lp => lp.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId.Value && lb.Year == year)
            .ToListAsync(cancellationToken);

        var annualBalance = FindBalance(balances, "Annual", "سنوية");
        var casualBalance = FindBalance(balances, "Casual", "Emergency", "عارضة", "طارئة");
        var sickBalance = FindBalance(balances, "Sick", "مرض");

        var annualPolicy = annualBalance?.LeavePolicy;
        var sickPolicy = sickBalance?.LeavePolicy;

        var requestsQuery = _context.LeaveRequests
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.LeaveStatus)
            .Where(lr => lr.EmployeeId == employeeId.Value)
            .AsQueryable();

        if (request.Year.HasValue)
        {
            requestsQuery = requestsQuery.Where(lr => lr.StartDate.Year == year || lr.EndDate.Year == year);
        }

        var requestsHistory = await requestsQuery
            .OrderByDescending(lr => lr.StartDate)
            .ThenByDescending(lr => lr.EndDate)
            .Take(request.HistoryCount)
            .Select(lr => new LeaveRequestHistoryDto
            {
                LeaveRequestId = lr.Id,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeNameEn = lr.LeaveType.NameEn,
                LeaveTypeNameAr = lr.LeaveType.NameAr,
                StatusId = lr.LeaveStatusId,
                StatusNameEn = lr.LeaveStatus.NameEn,
                StatusNameAr = lr.LeaveStatus.NameAr,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays
            })
            .ToListAsync(cancellationToken);

        var dto = new MyLeaveDashboardDto
        {
            Year = year,
            RemainingBalance = annualBalance?.RemainingDays ?? 0m,
            CarryOverBalance = annualBalance?.CarriedForwardDays ?? 0m,
            AnnualLeaveBalance = annualBalance?.TotalDays ?? 0m,
            ConsumedDays = annualBalance?.UsedDays ?? 0m,
            CasualLeaveBalance = casualBalance?.RemainingDays ?? 0m,
            AnnualAccrualRules = annualPolicy == null
                ? null
                : new LeaveAccrualRulesDto
                {
                    LeavePolicyId = annualPolicy.Id,
                    PolicyNameEn = annualPolicy.NameEn,
                    PolicyNameAr = annualPolicy.NameAr,
                    DefaultDaysPerYear = annualPolicy.DefaultDaysPerYear,
                    MaxCarryForward = annualPolicy.MaxCarryForward,
                    MaxConsecutiveDays = annualPolicy.MaxConsecutiveDays,
                    MinDaysNotice = annualPolicy.MinDaysNotice,
                    RequiresApproval = annualPolicy.RequiresApproval,
                    RequiresManagerApproval = annualPolicy.RequiresManagerApproval,
                    RequiresHRApproval = annualPolicy.RequiresHRApproval,
                    IsPaid = annualPolicy.IsPaid,
                    Description = annualPolicy.Description
                },
            SickLeaveRequiredDocuments = sickPolicy == null
                ? null
                : new SickLeaveDocumentRequirementDto
                {
                    LeavePolicyId = sickPolicy.Id,
                    PolicyNameEn = sickPolicy.NameEn,
                    PolicyNameAr = sickPolicy.NameAr,
                    RequiresDocument = sickPolicy.RequiresDocument,
                    Description = sickPolicy.Description
                },
            RequestsHistory = requestsHistory
        };

        return new GenericResponse<MyLeaveDashboardDto>
        {
            Success = true,
            Message = "Leave dashboard retrieved successfully",
            Data = dto
        };
    }

    private static LeaveBalance? FindBalance(IEnumerable<LeaveBalance> balances, params string[] nameKeywords)
    {
        return balances.FirstOrDefault(b => nameKeywords.Any(keyword =>
            b.LeavePolicy.LeaveType.NameEn.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            b.LeavePolicy.LeaveType.NameAr.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }
}
