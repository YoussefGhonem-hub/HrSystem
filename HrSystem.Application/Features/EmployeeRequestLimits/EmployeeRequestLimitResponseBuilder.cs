using System;
using System.Collections.Generic;
using System.Linq;
using HrSystem.Application.Features.EmployeeRequestLimits.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequestLimits;

internal static class EmployeeRequestLimitResponseBuilder
{
    public static async Task<EmployeeRequestLimitsDto> BuildAsync(
        ApplicationDbContext context,
        Guid employeeId,
        string employeeCode,
        string employeeName,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var vacationLimitEntities = await context.EmployeeVacationLimits
            .AsNoTracking()
            .Include(l => l.VacationType)
            .Where(l => l.EmployeeId == employeeId)
            .OrderBy(l => l.VacationType.SortOrder)
            .ThenBy(l => l.VacationType.NameEn)
            .ToListAsync(cancellationToken);

        var vacationLimits = await BuildVacationLimitDtosAsync(
            context,
            employeeId,
            vacationLimitEntities,
            cancellationToken);

        var permissionLimitEntities = await context.EmployeePermissionLimits
            .AsNoTracking()
            .Include(l => l.PermissionType)
            .Where(l => l.EmployeeId == employeeId)
            .OrderBy(l => l.PermissionType.SortOrder)
            .ThenBy(l => l.PermissionType.NameEn)
            .ToListAsync(cancellationToken);

        var permissionLimits = await BuildPermissionLimitDtosAsync(
            context,
            employeeId,
            permissionLimitEntities,
            now,
            cancellationToken);

        var globalPermissionLimit = await BuildGlobalPermissionLimitDtoAsync(
            context,
            employeeId,
            now,
            cancellationToken);

        return new EmployeeRequestLimitsDto
        {
            EmployeeId = employeeId,
            EmployeeCode = employeeCode,
            EmployeeName = employeeName,
            VacationLimits = vacationLimits,
            PermissionLimits = permissionLimits,
            GlobalPermissionLimit = globalPermissionLimit
        };
    }

    private static async Task<IReadOnlyCollection<EmployeeVacationLimitDto>> BuildVacationLimitDtosAsync(
        ApplicationDbContext context,
        Guid employeeId,
        IReadOnlyCollection<Domain.Entities.Requests.EmployeeVacationLimit> vacationLimits,
        CancellationToken cancellationToken)
    {
        if (vacationLimits.Count == 0)
        {
            return Array.Empty<EmployeeVacationLimitDto>();
        }

        var vacationTypeIds = vacationLimits.Select(l => l.VacationTypeId).Distinct().ToList();

        var balanceProjections = await context.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.EmployeeId == employeeId && vacationTypeIds.Contains(b.VacationTypeId))
            .OrderByDescending(b => b.Year)
            .Select(b => new VacationBalanceProjection(
                b.VacationTypeId,
                b.Year,
                b.AllocatedDays,
                b.CarryOverDays,
                b.ManualAdjustmentDays,
                b.UsedDays))
            .ToListAsync(cancellationToken);

        var balanceLookup = new Dictionary<Guid, VacationBalanceProjection>();
        foreach (var projection in balanceProjections)
        {
            if (!balanceLookup.ContainsKey(projection.VacationTypeId))
            {
                balanceLookup[projection.VacationTypeId] = projection;
            }
        }

        var vacationDtos = vacationLimits
            .Select(limit =>
            {
                balanceLookup.TryGetValue(limit.VacationTypeId, out var balance);

                var usedDays = balance?.UsedDays;
                decimal? remaining = balance?.AvailableDays;

                if (remaining is null && limit.MaxDaysPerYear.HasValue)
                {
                    var usedValue = usedDays ?? 0m;
                    remaining = Math.Max(0, limit.MaxDaysPerYear.Value - usedValue);
                }

                return new EmployeeVacationLimitDto
                {
                    VacationTypeId = limit.VacationTypeId,
                    VacationTypeName = limit.VacationType.NameEn,
                    VacationTypeNameAr = limit.VacationType.NameAr,
                    MaxDaysPerYear = limit.MaxDaysPerYear,
                    BalanceYear = balance?.Year,
                    AllocatedDays = balance?.AllocatedDays,
                    UsedDays = usedDays,
                    RemainingDays = remaining,
                    Notes = limit.Notes
                };
            })
            .ToList();

        return vacationDtos;
    }

    private static async Task<IReadOnlyCollection<EmployeePermissionLimitDto>> BuildPermissionLimitDtosAsync(
        ApplicationDbContext context,
        Guid employeeId,
        IReadOnlyCollection<Domain.Entities.Requests.EmployeePermissionLimit> permissionLimits,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var approvedStatuses = new[] { EmployeeRequestStatus.Approved, EmployeeRequestStatus.Completed };

        // Get all permission types (both with and without employee-specific limits)
        var allPermissionTypes = await context.PermissionTypes
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.NameEn)
            .Select(p => new { 
                p.Id, 
                p.NameEn, 
                p.NameAr, 
                p.DefaultMonthlyHours,
                p.SortOrder 
            })
            .ToListAsync(cancellationToken);

        if (allPermissionTypes.Count == 0)
        {
            return Array.Empty<EmployeePermissionLimitDto>();
        }

        var permissionTypeIds = allPermissionTypes.Select(p => p.Id).ToList();

        var usageLookup = await context.PermissionRequestDetails
            .AsNoTracking()
            .Where(d => d.EmployeeRequest.EmployeeId == employeeId
                        && d.PermissionDate >= startOfMonth
                        && d.PermissionDate < endOfMonth
                        && approvedStatuses.Contains(d.EmployeeRequest.Status)
                        && permissionTypeIds.Contains(d.PermissionTypeId))
            .GroupBy(d => d.PermissionTypeId)
            .Select(g => new PermissionUsageProjection(g.Key, g.Sum(x => x.TotalHours)))
            .ToDictionaryAsync(x => x.PermissionTypeId, cancellationToken);

        var employeeLimitsLookup = permissionLimits.ToDictionary(l => l.PermissionTypeId);

        var permissionDtos = allPermissionTypes
            .Select(permType =>
            {
                var usedHours = usageLookup.TryGetValue(permType.Id, out var usage)
                    ? usage.TotalHours
                    : 0m;

                // Check employee-specific limit first, then fall back to type default
                var employeeLimit = employeeLimitsLookup.TryGetValue(permType.Id, out var empLimit) ? empLimit : null;
                var maxHours = employeeLimit?.MaxHoursPerMonth ?? permType.DefaultMonthlyHours;
                var notes = employeeLimit?.Notes;

                var remaining = maxHours.HasValue
                    ? Math.Max(0, maxHours.Value - usedHours)
                    : (decimal?)null;

                return new EmployeePermissionLimitDto
                {
                    PermissionTypeId = permType.Id,
                    PermissionTypeName = permType.NameEn,
                    PermissionTypeNameAr = permType.NameAr,
                    MaxHoursPerMonth = maxHours,
                    Year = startOfMonth.Year,
                    Month = startOfMonth.Month,
                    UsedHours = usedHours,
                    RemainingHours = remaining,
                    Notes = notes
                };
            })
            .ToList();

        return permissionDtos;
    }

    private static async Task<GlobalPermissionLimitDto?> BuildGlobalPermissionLimitDtoAsync(
        ApplicationDbContext context,
        Guid employeeId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var globalLimit = await context.EmployeeGlobalPermissionLimits
            .AsNoTracking()
            .Where(gl => gl.EmployeeId == employeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (globalLimit == null || !globalLimit.TotalMonthlyHours.HasValue)
        {
            return null;
        }

        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var approvedStatuses = new[] { EmployeeRequestStatus.Approved, EmployeeRequestStatus.Completed };

        // Calculate total hours used across ALL permission types this month
        var usedHours = await context.PermissionRequestDetails
            .AsNoTracking()
            .Where(d => d.EmployeeRequest.EmployeeId == employeeId
                        && d.PermissionDate >= startOfMonth
                        && d.PermissionDate < endOfMonth
                        && approvedStatuses.Contains(d.EmployeeRequest.Status))
            .SumAsync(d => d.TotalHours, cancellationToken);

        var remaining = Math.Max(0, globalLimit.TotalMonthlyHours.Value - usedHours);

        return new GlobalPermissionLimitDto
        {
            TotalMonthlyHours = globalLimit.TotalMonthlyHours,
            Year = startOfMonth.Year,
            Month = startOfMonth.Month,
            UsedHours = usedHours,
            RemainingHours = remaining,
            Notes = globalLimit.Notes
        };
    }

    private sealed record VacationBalanceProjection(
        Guid VacationTypeId,
        int Year,
        decimal AllocatedDays,
        decimal CarryOverDays,
        decimal ManualAdjustmentDays,
        decimal UsedDays)
    {
        public decimal AvailableDays => AllocatedDays + CarryOverDays + ManualAdjustmentDays - UsedDays;
    }

    private sealed record PermissionUsageProjection(Guid PermissionTypeId, decimal TotalHours);
}
