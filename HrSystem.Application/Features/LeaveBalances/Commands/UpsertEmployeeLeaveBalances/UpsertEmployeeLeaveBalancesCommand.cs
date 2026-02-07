using ErrorOr;
using HrSystem.Application.Features.LeaveBalances.Dtos;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.LeaveBalances.Commands.UpsertEmployeeLeaveBalances;

public record UpsertEmployeeLeaveBalancesCommand(
    Guid EmployeeId,
    int Year,
    IReadOnlyCollection<LeaveBalanceAllocationPayload> Allocations
) : IRequest<ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>>;

public record LeaveBalanceAllocationPayload(
    Guid VacationTypeId,
    decimal AllocatedDays,
    decimal? CarryOverDays,
    decimal? ManualAdjustmentDays,
    string? Notes
);

public class UpsertEmployeeLeaveBalancesCommandHandler
    : IRequestHandler<UpsertEmployeeLeaveBalancesCommand, ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpsertEmployeeLeaveBalancesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeLeaveHistoryDto>>> Handle(
        UpsertEmployeeLeaveBalancesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Allocations.Count == 0)
        {
            return Error.Validation(description: "At least one leave allocation payload is required.");
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found.");
        }

        var vacationTypeIds = request.Allocations.Select(a => a.VacationTypeId).Distinct().ToList();

        var vacationTypes = await _context.VacationTypes
            .AsNoTracking()
            .Where(v => vacationTypeIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        if (vacationTypes.Count != vacationTypeIds.Count)
        {
            return Error.NotFound(description: "One or more vacation types were not found.");
        }

        var existingBalances = await _context.EmployeeLeaveBalances
            .Where(b => b.EmployeeId == request.EmployeeId
                        && b.Year == request.Year
                        && vacationTypeIds.Contains(b.VacationTypeId))
            .ToListAsync(cancellationToken);

        foreach (var allocation in request.Allocations)
        {
            var allocationNotes = allocation.Notes?.Trim();
            var targetCarryOver = allocation.CarryOverDays ?? 0m;
            var targetManualAdjustment = allocation.ManualAdjustmentDays ?? 0m;

            var balance = existingBalances.FirstOrDefault(b => b.VacationTypeId == allocation.VacationTypeId);
            var previousAvailable = balance?.CalculateAvailableDays() ?? 0m;

            if (balance is null)
            {
                balance = new EmployeeLeaveBalance
                {
                    EmployeeId = request.EmployeeId,
                    VacationTypeId = allocation.VacationTypeId,
                    Year = request.Year,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId,
                    AllocatedDays = allocation.AllocatedDays,
                    CarryOverDays = targetCarryOver,
                    ManualAdjustmentDays = targetManualAdjustment,
                    UsedDays = 0m,
                    Notes = allocationNotes
                };

                existingBalances.Add(balance);
                await _context.EmployeeLeaveBalances.AddAsync(balance, cancellationToken);
            }
            else
            {
                balance.AllocatedDays = allocation.AllocatedDays;
                balance.CarryOverDays = targetCarryOver;
                balance.ManualAdjustmentDays = targetManualAdjustment;
                balance.Notes = allocationNotes;

                if (!balance.BranchId.HasValue)
                {
                    balance.BranchId = employee.BranchId;
                }
            }

            var newAvailable = balance.CalculateAvailableDays();
            var delta = newAvailable - previousAvailable;

            if (delta != 0m)
            {
                var transactionType = delta >= 0m
                    ? LeaveTransactionType.Allocation
                    : LeaveTransactionType.Adjustment;

                var transaction = new EmployeeLeaveTransaction
                {
                    EmployeeLeaveBalanceId = balance.Id,
                    EmployeeId = balance.EmployeeId,
                    VacationTypeId = balance.VacationTypeId,
                    Year = balance.Year,
                    TransactionType = transactionType,
                    DaysChanged = delta,
                    BalanceAfter = newAvailable,
                    ReferenceType = "ManualAllocation",
                    ReferenceId = null,
                    Notes = allocationNotes,
                    TenantId = balance.TenantId,
                    BranchId = balance.BranchId
                };

                await _context.EmployeeLeaveTransactions.AddAsync(transaction, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var balances = await _context.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.EmployeeId == request.EmployeeId && b.Year == request.Year)
            .OrderBy(b => b.VacationType.SortOrder)
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
                Transactions = b.Transactions
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
            "Leave balances upserted successfully.");
    }
}
