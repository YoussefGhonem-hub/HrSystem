using ErrorOr;
using HrSystem.Application.Features.LeaveBalances.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveBalanceSummary;

public record GetEmployeeLeaveBalanceSummaryQuery(
    Guid? EmployeeId = null,
    int? Year = null
) : IRequest<ErrorOr<GenericResponse<EmployeeLeaveBalanceSummaryDto>>>;

public class GetEmployeeLeaveBalanceSummaryQueryHandler
    : IRequestHandler<GetEmployeeLeaveBalanceSummaryQuery, ErrorOr<GenericResponse<EmployeeLeaveBalanceSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeLeaveBalanceSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeLeaveBalanceSummaryDto>>> Handle(
        GetEmployeeLeaveBalanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // If EmployeeId is not provided, use current user's employee ID
        Guid? employeeId = request.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            employeeId = CurrentUser.EmployeeId;

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
                return Error.Unauthorized(description: "Current user is not linked to an employee.");
            }
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId.Value, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found.");
        }

        var year = request.Year ?? DateTime.UtcNow.Year;

        // Get all leave balances for the specified employee and year
        var balances = await _context.EmployeeLeaveBalances
            .AsNoTracking()
            .Include(b => b.VacationType)
            .Where(b => b.EmployeeId == employeeId.Value && b.Year == year)
            .OrderBy(b => b.VacationType.SortOrder)
            .ThenBy(b => b.VacationType.NameEn)
            .ToListAsync(cancellationToken);

        if (!balances.Any())
        {
            return Error.NotFound(description: $"No leave balances found for employee in year {year}.");
        }

        // Calculate totals
        decimal totalRemainingBalance = 0;
        decimal totalCarryOverDays = 0;
        decimal totalConsumedDays = 0;
        decimal totalAllocatedDays = 0;

        var leaveTypeBalances = balances.Select(b =>
        {
            var availableDays = b.CalculateAvailableDays();
            totalRemainingBalance += availableDays;
            totalCarryOverDays += b.CarryOverDays;
            totalConsumedDays += b.UsedDays;
            totalAllocatedDays += b.AllocatedDays;

            return new LeaveTypeBalanceDto
            {
                VacationTypeId = b.VacationTypeId,
                VacationTypeName = b.VacationType.NameEn,
                VacationTypeNameAr = b.VacationType.NameAr,
                AllocatedDays = b.AllocatedDays,
                CarryOverDays = b.CarryOverDays,
                ManualAdjustmentDays = b.ManualAdjustmentDays,
                UsedDays = b.UsedDays,
                AvailableDays = availableDays
            };
        }).ToList();

        var summary = new EmployeeLeaveBalanceSummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullNameEn,
            Year = year,
            RemainingBalance = totalRemainingBalance,
            CarryOverBalance = totalCarryOverDays,
            AnnualLeavesBalance = totalAllocatedDays,
            ConsumedDays = totalConsumedDays,
            LeaveTypeBalances = leaveTypeBalances
        };

        return GenericResponse<EmployeeLeaveBalanceSummaryDto>.SuccessResult(
            summary,
            "Leave balance summary retrieved successfully.");
    }
}
