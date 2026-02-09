using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.LeaveBalances.Dtos;

public record EmployeeLeaveHistoryDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public IReadOnlyCollection<EmployeeLeaveBalanceDto> Balances { get; init; } = Array.Empty<EmployeeLeaveBalanceDto>();
}

public record EmployeeLeaveBalanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid VacationTypeId { get; init; }
    public string VacationTypeName { get; init; } = string.Empty;
    public string? VacationTypeNameAr { get; init; }
    public int Year { get; init; }
    public decimal AllocatedDays { get; init; }
    public decimal CarryOverDays { get; init; }
    public decimal ManualAdjustmentDays { get; init; }
    public decimal UsedDays { get; init; }
    public decimal AvailableDays { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyCollection<EmployeeLeaveTransactionDto> Transactions { get; init; } = Array.Empty<EmployeeLeaveTransactionDto>();
}

public record EmployeeLeaveTransactionDto
{
    public Guid Id { get; init; }
    public LeaveTransactionType TransactionType { get; init; }
    public decimal DaysChanged { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public Guid? CreatedBy { get; init; }
}

public record EmployeeLeaveBalanceSummaryDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal RemainingBalance { get; init; }
    public decimal CarryOverBalance { get; init; }
    public decimal AnnualLeavesBalance { get; init; }
    public decimal ConsumedDays { get; init; }
    public IReadOnlyCollection<LeaveTypeBalanceDto> LeaveTypeBalances { get; init; } = Array.Empty<LeaveTypeBalanceDto>();
}

public record LeaveTypeBalanceDto
{
    public Guid VacationTypeId { get; init; }
    public string VacationTypeName { get; init; } = string.Empty;
    public string? VacationTypeNameAr { get; init; }
    public decimal AllocatedDays { get; init; }
    public decimal CarryOverDays { get; init; }
    public decimal ManualAdjustmentDays { get; init; }
    public decimal UsedDays { get; init; }
    public decimal AvailableDays { get; init; }
}
