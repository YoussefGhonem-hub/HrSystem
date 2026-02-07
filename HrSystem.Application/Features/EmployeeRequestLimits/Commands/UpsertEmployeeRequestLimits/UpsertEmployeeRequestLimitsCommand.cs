using System;
using System.Collections.Generic;
using System.Linq;
using ErrorOr;
using HrSystem.Application.Features.EmployeeRequestLimits;
using HrSystem.Application.Features.EmployeeRequestLimits.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequestLimits.Commands.UpsertEmployeeRequestLimits;

public record UpsertEmployeeRequestLimitsCommand(
    Guid EmployeeId,
    IReadOnlyCollection<VacationLimitPayload>? VacationLimits,
    IReadOnlyCollection<PermissionLimitPayload>? PermissionLimits
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>>;

public record VacationLimitPayload(
    Guid VacationTypeId,
    int? MaxDaysPerYear,
    string? Notes
);

public record PermissionLimitPayload(
    Guid PermissionTypeId,
    decimal? MaxHoursPerMonth,
    string? Notes
);

public class UpsertEmployeeRequestLimitsCommandHandler
    : IRequestHandler<UpsertEmployeeRequestLimitsCommand, ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpsertEmployeeRequestLimitsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>> Handle(
        UpsertEmployeeRequestLimitsCommand request,
        CancellationToken cancellationToken)
    {
        if ((request.VacationLimits is null || request.VacationLimits.Count == 0) &&
            (request.PermissionLimits is null || request.PermissionLimits.Count == 0))
        {
            return Error.Validation(description: "At least one limit payload is required.");
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == request.EmployeeId)
            .Select(e => new EmployeeSummary(
                e.Id,
                e.TenantId,
                e.BranchId,
                e.EmployeeCode,
                (e.FirstNameEn + " " + e.LastNameEn).Trim(),
                (e.FirstNameAr + " " + e.LastNameAr).Trim()))
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found.");
        }

        var vacationPayloads = request.VacationLimits?.ToList() ?? new List<VacationLimitPayload>();
        var permissionPayloads = request.PermissionLimits?.ToList() ?? new List<PermissionLimitPayload>();

        if (vacationPayloads.Count > 0)
        {
            var vacationTypeIds = vacationPayloads.Select(v => v.VacationTypeId).Distinct().ToList();
            var existingVacationTypes = await _context.VacationTypes
                .AsNoTracking()
                .Where(v => vacationTypeIds.Contains(v.Id))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            if (existingVacationTypes.Count != vacationTypeIds.Count)
            {
                return Error.NotFound(description: "One or more vacation types were not found.");
            }
        }

        if (permissionPayloads.Count > 0)
        {
            var permissionTypeIds = permissionPayloads.Select(v => v.PermissionTypeId).Distinct().ToList();
            var existingPermissionTypes = await _context.PermissionTypes
                .AsNoTracking()
                .Where(v => permissionTypeIds.Contains(v.Id))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            if (existingPermissionTypes.Count != permissionTypeIds.Count)
            {
                return Error.NotFound(description: "One or more permission types were not found.");
            }
        }

        await UpsertVacationLimitsAsync(vacationPayloads, employee, cancellationToken);
        await UpsertPermissionLimitsAsync(permissionPayloads, employee, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var employeeName = ResolveEmployeeName(employee);
        var response = await EmployeeRequestLimitResponseBuilder.BuildAsync(
            _context,
            employee.Id,
            employee.EmployeeCode,
            employeeName,
            cancellationToken);

        return GenericResponse<EmployeeRequestLimitsDto>.SuccessResult(
            response,
            "Employee request limits saved successfully.");
    }

    private async Task UpsertVacationLimitsAsync(
        IReadOnlyList<VacationLimitPayload> vacationPayloads,
        EmployeeSummary employee,
        CancellationToken cancellationToken)
    {
        if (vacationPayloads.Count == 0)
        {
            return;
        }

        var vacationTypeIds = vacationPayloads.Select(v => v.VacationTypeId).Distinct().ToList();

        var existingVacationLimits = await _context.EmployeeVacationLimits
            .Where(l => l.EmployeeId == employee.Id && vacationTypeIds.Contains(l.VacationTypeId))
            .ToListAsync(cancellationToken);

        foreach (var payload in vacationPayloads)
        {
            var notes = payload.Notes?.Trim();
            var existing = existingVacationLimits.FirstOrDefault(l => l.VacationTypeId == payload.VacationTypeId);

            if (!payload.MaxDaysPerYear.HasValue)
            {
                if (existing is not null)
                {
                    _context.EmployeeVacationLimits.Remove(existing);
                }

                continue;
            }

            if (existing is null)
            {
                existing = new EmployeeVacationLimit
                {
                    EmployeeId = employee.Id,
                    VacationTypeId = payload.VacationTypeId,
                    MaxDaysPerYear = payload.MaxDaysPerYear,
                    Notes = notes,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId
                };

                await _context.EmployeeVacationLimits.AddAsync(existing, cancellationToken);
                existingVacationLimits.Add(existing);
            }
            else
            {
                existing.MaxDaysPerYear = payload.MaxDaysPerYear;
                existing.Notes = notes;

                if (!existing.BranchId.HasValue)
                {
                    existing.BranchId = employee.BranchId;
                }
            }
        }
    }

    private async Task UpsertPermissionLimitsAsync(
        IReadOnlyList<PermissionLimitPayload> permissionPayloads,
        EmployeeSummary employee,
        CancellationToken cancellationToken)
    {
        if (permissionPayloads.Count == 0)
        {
            return;
        }

        var permissionTypeIds = permissionPayloads.Select(v => v.PermissionTypeId).Distinct().ToList();

        var existingPermissionLimits = await _context.EmployeePermissionLimits
            .Where(l => l.EmployeeId == employee.Id && permissionTypeIds.Contains(l.PermissionTypeId))
            .ToListAsync(cancellationToken);

        foreach (var payload in permissionPayloads)
        {
            var notes = payload.Notes?.Trim();
            var existing = existingPermissionLimits.FirstOrDefault(l => l.PermissionTypeId == payload.PermissionTypeId);

            if (!payload.MaxHoursPerMonth.HasValue)
            {
                if (existing is not null)
                {
                    _context.EmployeePermissionLimits.Remove(existing);
                }

                continue;
            }

            if (existing is null)
            {
                existing = new EmployeePermissionLimit
                {
                    EmployeeId = employee.Id,
                    PermissionTypeId = payload.PermissionTypeId,
                    MaxHoursPerMonth = payload.MaxHoursPerMonth,
                    Notes = notes,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId
                };

                await _context.EmployeePermissionLimits.AddAsync(existing, cancellationToken);
                existingPermissionLimits.Add(existing);
            }
            else
            {
                existing.MaxHoursPerMonth = payload.MaxHoursPerMonth;
                existing.Notes = notes;

                if (!existing.BranchId.HasValue)
                {
                    existing.BranchId = employee.BranchId;
                }
            }
        }
    }

    private static string ResolveEmployeeName(EmployeeSummary employee)
    {
        if (!string.IsNullOrWhiteSpace(employee.EnglishName))
        {
            return employee.EnglishName;
        }

        if (!string.IsNullOrWhiteSpace(employee.ArabicName))
        {
            return employee.ArabicName;
        }

        return employee.EmployeeCode;
    }

    private sealed record EmployeeSummary(
        Guid Id,
        Guid TenantId,
        Guid? BranchId,
        string EmployeeCode,
        string EnglishName,
        string ArabicName);
}
