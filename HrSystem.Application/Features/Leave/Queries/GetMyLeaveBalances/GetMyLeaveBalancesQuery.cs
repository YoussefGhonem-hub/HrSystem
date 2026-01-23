using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveBalances;

/// <summary>
/// Query to get leave balances for the currently logged-in user
/// </summary>
public record GetMyLeaveBalancesQuery(int? Year = null) : IRequest<ErrorOr<GenericResponse<List<LeaveBalanceDto>>>>;

public class GetMyLeaveBalancesQueryHandler : IRequestHandler<GetMyLeaveBalancesQuery, ErrorOr<GenericResponse<List<LeaveBalanceDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyLeaveBalancesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<LeaveBalanceDto>>>> Handle(
        GetMyLeaveBalancesQuery request,
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
            return Error.Unauthorized("LeaveBalance.Unauthorized", "Current user is not linked to an employee");
        }

        var year = request.Year ?? DateTime.UtcNow.Year;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeavePolicy)
                .ThenInclude(lp => lp.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId.Value && lb.Year == year)
            .OrderBy(lb => lb.LeavePolicy.LeaveType.DisplayOrder)
            .ThenBy(lb => lb.LeavePolicy.NameEn)
            .Select(lb => new LeaveBalanceDto
            {
                LeavePolicyId = lb.LeavePolicyId,
                LeavePolicyNameEn = lb.LeavePolicy.NameEn,
                LeavePolicyNameAr = lb.LeavePolicy.NameAr,
                LeaveTypeId = lb.LeavePolicy.LeaveTypeId,
                LeaveTypeNameEn = lb.LeavePolicy.LeaveType.NameEn,
                LeaveTypeNameAr = lb.LeavePolicy.LeaveType.NameAr,
                Year = lb.Year,
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                RemainingDays = lb.RemainingDays,
                CarriedForwardDays = lb.CarriedForwardDays
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<LeaveBalanceDto>>
        {
            Success = true,
            Message = "Leave balances retrieved successfully",
            Data = balances
        };
    }
}

