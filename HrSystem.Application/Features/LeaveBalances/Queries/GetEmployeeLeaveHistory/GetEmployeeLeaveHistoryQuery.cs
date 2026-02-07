using System;
using ErrorOr;
using HrSystem.Application.Features.LeaveBalances.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveHistory;

public record GetEmployeeLeaveHistoryQuery(
    Guid EmployeeId,
    Guid? VacationTypeId = null,
    int? FromYear = null,
    int? ToYear = null,
    bool IncludeTransactions = false
) : IRequest<ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>>;

public class GetEmployeeLeaveHistoryQueryHandler
    : IRequestHandler<GetEmployeeLeaveHistoryQuery, ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeLeaveHistoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>> Handle(
        GetEmployeeLeaveHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found.");
        }

        var balancesQuery = _context.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.EmployeeId == request.EmployeeId);

        if (request.VacationTypeId.HasValue)
        {
            balancesQuery = balancesQuery.Where(b => b.VacationTypeId == request.VacationTypeId.Value);
        }

        if (request.FromYear.HasValue)
        {
            balancesQuery = balancesQuery.Where(b => b.Year >= request.FromYear.Value);
        }

        if (request.ToYear.HasValue)
        {
            balancesQuery = balancesQuery.Where(b => b.Year <= request.ToYear.Value);
        }

        var includeTransactions = request.IncludeTransactions;

        var balances = await balancesQuery
            .OrderByDescending(b => b.Year)
            .ThenBy(b => b.VacationType.SortOrder)
            .ThenBy(b => b.VacationType.NameEn)
            .Select(b => new EmployeeLeaveBalanceDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                VacationTypeId = b.VacationTypeId,
                VacationTypeName = b.VacationType.NameEn,
                VacationTypeNameAr = b.VacationType.NameAr,
                Year = b.Year,
                AllocatedDays = b.AllocatedDays,
                CarryOverDays = b.CarryOverDays,
                ManualAdjustmentDays = b.ManualAdjustmentDays,
                UsedDays = b.UsedDays,
                AvailableDays = b.AllocatedDays + b.CarryOverDays + b.ManualAdjustmentDays - b.UsedDays,
                Notes = b.Notes,
                Transactions = includeTransactions
                    ? b.Transactions
                        .OrderByDescending(t => t.CreatedDate)
                        .Take(25)
                        .Select(t => new EmployeeLeaveTransactionDto
                        {
                            Id = t.Id,
                            TransactionType = t.TransactionType,
                            DaysChanged = t.DaysChanged,
                            BalanceAfter = t.BalanceAfter,
                            ReferenceType = t.ReferenceType,
                            ReferenceId = t.ReferenceId,
                            Notes = t.Notes,
                            CreatedDate = t.CreatedDate,
                            CreatedBy = t.CreatedBy
                        })
                        .ToList()
                    : Array.Empty<EmployeeLeaveTransactionDto>()
            })
            .ToListAsync(cancellationToken);

        var response = new EmployeeLeaveHistoryDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullNameEn,
            Balances = balances
        };

        return GenericResponse<EmployeeLeaveHistoryDto>.SuccessResult(
            response,
            "Leave balance history retrieved successfully.");
    }
}
