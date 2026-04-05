using System;

namespace HrSystem.Application.Features.EmployeeRequestLimits.Dtos;

public class EmployeeRequestLimitsDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public IReadOnlyCollection<EmployeeVacationLimitDto> VacationLimits { get; init; } = Array.Empty<EmployeeVacationLimitDto>();
    public IReadOnlyCollection<EmployeePermissionLimitDto> PermissionLimits { get; init; } = Array.Empty<EmployeePermissionLimitDto>();
    public GlobalPermissionLimitDto? GlobalPermissionLimit { get; init; }
}

public class EmployeeVacationLimitDto
{
    public Guid VacationTypeId { get; init; }
    public string? VacationTypeName { get; init; }
    public string? VacationTypeNameAr { get; init; }
    public int? MaxDaysPerYear { get; init; }
    public int? BalanceYear { get; init; }
    public decimal? AllocatedDays { get; init; }
    public decimal? UsedDays { get; init; }
    public decimal? RemainingDays { get; init; }
    public string? Notes { get; init; }
}

public class EmployeePermissionLimitDto
{
    public Guid PermissionTypeId { get; init; }
    public string? PermissionTypeName { get; init; }
    public string? PermissionTypeNameAr { get; init; }
    public decimal? MaxHoursPerMonth { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal UsedHours { get; init; }
    public decimal? RemainingHours { get; init; }
    public string? Notes { get; init; }
}

public class GlobalPermissionLimitDto
{
    public decimal? TotalMonthlyHours { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal UsedHours { get; init; }
    public decimal? RemainingHours { get; init; }
    public string? Notes { get; init; }
}
