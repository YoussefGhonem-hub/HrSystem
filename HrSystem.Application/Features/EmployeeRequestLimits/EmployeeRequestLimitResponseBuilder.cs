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

        return new EmployeeRequestLimitsDto
        {
            EmployeeId = employeeId,
            EmployeeCode = employeeCode,
            EmployeeName = employeeName,
            VacationLimits = vacationLimits,
            PermissionLimits = permissionLimits
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
        if (permissionLimits.Count == 0)
        {
            return Array.Empty<EmployeePermissionLimitDto>();
        }

        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var approvedStatuses = new[] { EmployeeRequestStatus.Approved, EmployeeRequestStatus.Completed };

        var usageLookup = await context.PermissionRequestDetails
            .AsNoTracking()
            .Where(d => d.EmployeeRequest.EmployeeId == employeeId
                        && d.PermissionDate >= startOfMonth
                        && d.PermissionDate < endOfMonth
                        && approvedStatuses.Contains(d.EmployeeRequest.Status))
            .GroupBy(d => d.PermissionTypeId)
            .Select(g => new PermissionUsageProjection(g.Key, g.Sum(x => x.TotalHours)))
            .ToDictionaryAsync(x => x.PermissionTypeId, cancellationToken);

        var permissionDtos = permissionLimits
            .Select(limit =>
            {
                var usedHours = usageLookup.TryGetValue(limit.PermissionTypeId, out var usage)
                    ? usage.TotalHours
                    : 0m;

                var remaining = limit.MaxHoursPerMonth.HasValue
                    ? Math.Max(0, limit.MaxHoursPerMonth.Value - usedHours)
                    : (decimal?)null;

                return new EmployeePermissionLimitDto
                {
                    PermissionTypeId = limit.PermissionTypeId,
                    PermissionTypeName = limit.PermissionType.NameEn,
                    PermissionTypeNameAr = limit.PermissionType.NameAr,
                    MaxHoursPerMonth = limit.MaxHoursPerMonth,
                    Year = startOfMonth.Year,
                    Month = startOfMonth.Month,
                    UsedHours = usedHours,
                    RemainingHours = remaining,
                    Notes = limit.Notes
                };
            })
            .ToList();

        return permissionDtos;
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
