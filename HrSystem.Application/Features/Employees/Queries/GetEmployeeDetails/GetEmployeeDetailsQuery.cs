using System;
using System.Collections.Generic;
using System.Linq;
using ErrorOr;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;

public record GetEmployeeDetailsQuery(Guid EmployeeId, int AttendanceRecentCount = 10)
    : IRequest<ErrorOr<GenericResponse<EmployeeDetailsDto>>>;

public class GetEmployeeDetailsQueryHandler : IRequestHandler<GetEmployeeDetailsQuery, ErrorOr<GenericResponse<EmployeeDetailsDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public GetEmployeeDetailsQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDetailsDto>>> Handle(GetEmployeeDetailsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetEmployeeAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        if (!CurrentUser.IsSuperAdmin && await IsAdminProfileAsync(employee.UserId, cancellationToken))
        {
            return Error.Forbidden("Employee.AdminProfileViewForbidden", "You are not allowed to view admin profiles.");
        }

        var payroll = await GetEmployeePayrollDetailsAsync(request.EmployeeId, cancellationToken);
        var payrollHistory = await GetPayrollHistoryAsync(request.EmployeeId, cancellationToken);
        var attendance = await GetAttendanceConfigurationAsync(request.EmployeeId, cancellationToken);
        var attendanceHistory = await GetAttendanceHistoryAsync(request.EmployeeId, request.AttendanceRecentCount, cancellationToken);
        var documents = await GetEmployeeDocumentsAsync(request.EmployeeId, cancellationToken);
        var assets = await GetEmployeeAssetsAsync(request.EmployeeId, cancellationToken);
        var balanceSnapshot = await GetEmployeeBalanceSnapshotAsync(request.EmployeeId, cancellationToken);

        if (payroll is null)
        {
            payroll = new EmployeePayrollDetailsDto();
        }
        payroll.PayrollHistory = payrollHistory;

        if (attendance is null)
        {
            attendance = new EmployeeAttendanceDetailsDto();
        }
        attendance.AttendanceHistory = attendanceHistory;

        var personalInfo = await MapPersonalInfoAsync(employee, cancellationToken);

        var dto = new EmployeeDetailsDto
        {
            PersonalInfo = personalInfo,
            JobInfo = await MapJobInfoAsync(employee, cancellationToken),
            Payroll = payroll,
            Attendance = attendance,
            Documents = documents,
            Assets = assets,
            Balances = balanceSnapshot
        };

        return new GenericResponse<EmployeeDetailsDto>
        {
            Success = true,
            Message = "Employee details retrieved successfully",
            Data = dto
        };
    }

    private async Task<HrSystem.Domain.Entities.Employee.Employee?> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        return employee;
    }

    private async Task<EmployeePersonalInfoDetailsDto> MapPersonalInfoAsync(HrSystem.Domain.Entities.Employee.Employee employee, CancellationToken cancellationToken)
    {
        var profileUrl = await ResolveProfileImageUrl(employee.ProfilePictureUrl, cancellationToken);
        var roles = await GetRolesAsync(employee.UserId, cancellationToken);

        return new EmployeePersonalInfoDetailsDto
        {
            FirstNameAr = employee.FirstNameAr,
            LastNameAr = employee.LastNameAr,
            FirstNameEn = employee.FirstNameEn,
            LastNameEn = employee.LastNameEn,
            ProfilePictureUrl = profileUrl,
            Roles = roles,
            NationalId = employee.NationalId,
            PassportNumber = employee.PassportNumber,
            DateOfBirth = employee.DateOfBirth,
            GenderId = employee.GenderId,
            MaritalStatusId = employee.MaritalStatusId,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            MobileNumber = employee.MobileNumber,
            AddressAr = employee.AddressAr,
            AddressEn = employee.AddressEn,
            City = employee.City,
            Country = employee.Country
        };
    }

    private async Task<List<EmployeeRoleDto>> GetRolesAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return new List<EmployeeRoleDto>();
        }

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new EmployeeRoleDto
                {
                    Id = r.Id,
                    NameEn = r.Name ?? string.Empty,
                    NameAr = r.DisplayName ?? r.Name
                })
            .ToListAsync(cancellationToken);

        return roles;
    }

    private async Task<bool> IsAdminProfileAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(_context.Roles,
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.Any(name =>
            string.Equals(name, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string?> ResolveProfileImageUrl(string? storedValue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storedValue)) return null;
        if (storedValue.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return storedValue;

        try
        {
            var url = await _storageService.DownloadFileUrl(storedValue, cancellationToken);
            return string.IsNullOrWhiteSpace(url) ? storedValue : url;
        }
        catch
        {
            // If presign fails, return stored key to avoid blocking the response
            return storedValue;
        }
    }

    private async Task<EmployeeJobInfoDetailsDto> MapJobInfoAsync(HrSystem.Domain.Entities.Employee.Employee employee, CancellationToken cancellationToken)
    {
        Guid? roleId = null;
        string? roleNameEn = null;
        string? roleNameAr = null;

        if (employee.UserId.HasValue)
        {
            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == employee.UserId.Value)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .FirstOrDefaultAsync(cancellationToken);

            if (userRole != null)
            {
                roleId = userRole.Id;
                roleNameEn = userRole.Name ?? string.Empty;
                roleNameAr = userRole.DisplayName ?? userRole.Name;
            }
        }

        return new EmployeeJobInfoDetailsDto
        {
            EmployeeCode = employee.EmployeeCode,
            StatusId = employee.StatusId,
            EmploymentStatusNameEn = employee.Status?.NameEn ?? string.Empty,
            EmploymentStatusNameAr = employee.Status?.NameAr ?? string.Empty,
            DepartmentId = employee.DepartmentId,
            JobTitleId = employee.JobTitleId,
            DirectManagerId = employee.DirectManagerId,
            BranchId = employee.BranchId,
            ContractTypeId = employee.ContractTypeId,
            HiringDate = employee.HiringDate,
            ProbationPeriodMonths = employee.ProbationPeriodMonths,
            RoleId = roleId,
            RoleNameEn = roleNameEn,
            RoleNameAr = roleNameAr
        };
    }

    private async Task<EmployeePayrollDetailsDto?> GetEmployeePayrollDetailsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
            .Include(s => s.Allowances)
            .Include(s => s.Deductions)
            .Where(s => s.EmployeeId == employeeId && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (salary is null)
        {
            return null;
        }

        var activeAllowances = salary.Allowances.Where(a => !a.IsDeleted).ToList();
        var activeDeductions = salary.Deductions.Where(d => !d.IsDeleted).ToList();

        var grossSalary = salary.BasicSalary + activeAllowances.Sum(a =>
            a.IsPercentage ? salary.BasicSalary * (a.PercentageValue ?? 0m) / 100m : a.Amount);

        var deductionTotal = activeDeductions.Sum(d =>
            d.IsPercentage ? grossSalary * (d.PercentageValue ?? 0m) / 100m : d.Amount);

        var socialInsuranceContribution = salary.IsSocialInsuranceEnabled && salary.SocialInsuranceEmployeeRate.HasValue
            ? salary.BasicSalary * salary.SocialInsuranceEmployeeRate.Value / 100m
            : 0m;

        var netSalary = grossSalary - deductionTotal - socialInsuranceContribution;

        var dto = new EmployeePayrollDetailsDto
        {
            BasicSalary = salary.BasicSalary,
            GrossSalary = grossSalary,
            NetSalary = netSalary,
            EffectiveDate = salary.EffectiveDate,
            Currency = salary.Currency,
            IncludeSocialInsurance = salary.IsSocialInsuranceEnabled,
            SocialInsuranceEmployeeRate = salary.SocialInsuranceEmployeeRate,
            SocialInsuranceEmployerRate = salary.SocialInsuranceEmployerRate,
            PaymentMethod = salary.PaymentMethod,
            BankInfo = BuildBankInfoPayload(
                salary.BankName,
                salary.BankBranch,
                salary.BankAccountNumber,
                salary.BankIban,
                salary.BankSwiftCode),
            Notes = salary.Notes,
            OvertimeMultiplier = salary.OvertimeMultiplier,
            Allowances = activeAllowances
                .Select(a => new PayrollAllowancePayload { NameAr = a.NameAr, NameEn = a.NameEn, Description = a.Description, IsTaxable = a.IsTaxable, IsSubjectToInsurance = a.IsSubjectToInsurance, Amount = a.Amount, IsPercentage = a.IsPercentage, PercentageValue = a.PercentageValue })
                .ToList(),
            Deductions = activeDeductions
                .Select(d => new PayrollDeductionPayload { NameAr = d.NameAr, NameEn = d.NameEn, Description = d.Description, IsRecurring = d.IsRecurring, Amount = d.Amount, IsPercentage = d.IsPercentage, PercentageValue = d.PercentageValue })
                .ToList()
        };

        return dto;
    }

    private async Task<List<EmployeePayslipHistoryItemDto>> GetPayrollHistoryAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .Take(12)
            .Select(p => new EmployeePayslipHistoryItemDto
            {
                PayslipId = p.Id,
                CycleName = p.PayrollCycle.CycleName,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = p.IsPaid ? "Paid" : "Processing",
                GeneratedDate = p.GeneratedDate,
                PaidDate = p.PaidDate
            })
            .ToListAsync(cancellationToken);
    }

    private static PayrollBankInfoPayload? BuildBankInfoPayload(
        string? bankName,
        string? bankBranch,
        string? accountNumber,
        string? iban,
        string? swiftCode)
    {
        var hasBankInfo = !string.IsNullOrWhiteSpace(bankName)
            || !string.IsNullOrWhiteSpace(bankBranch)
            || !string.IsNullOrWhiteSpace(accountNumber)
            || !string.IsNullOrWhiteSpace(iban)
            || !string.IsNullOrWhiteSpace(swiftCode);

        if (!hasBankInfo)
        {
            return null;
        }

        return new PayrollBankInfoPayload { BankName = bankName, BankBranch = bankBranch, AccountNumber = accountNumber, Iban = iban, SwiftCode = swiftCode };
    }

    private async Task<EmployeeAttendanceDetailsDto?> GetAttendanceConfigurationAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var configuration = await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.IsConfigurationRecord)
            .OrderByDescending(a => a.Date)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuration is null)
        {
            return null;
        }

        return new EmployeeAttendanceDetailsDto
        {
            WorkShift = configuration.WorkShift,
            WorkDays = configuration.WorkDays,
            GracePeriod = configuration.GracePeriod,
            MaxLatePerMonth = configuration.MaxLatePerMonth,
            OvertimeEligible = configuration.OvertimeEligible,
            AttendanceMethod = configuration.AttendanceMethod,
            LateDeductionPolicy = configuration.LateDeductionPolicy,
            AbsenceDeductionPolicy = configuration.AbsenceDeductionPolicy,
            HalfDayRule = configuration.HalfDayRule,
            MissingCheckoutHandling = configuration.MissingCheckoutHandling
        };
    }

    private async Task<List<EmployeeAttendanceHistoryItemDto>> GetAttendanceHistoryAsync(Guid employeeId, int count, CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            count = 10;
        }

        return await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && !a.IsConfigurationRecord)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CheckInTime)
            .Take(count)
            .Select(a => new EmployeeAttendanceHistoryItemDto
            {
                Id = a.Id,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                StatusId = a.StatusId,
                StatusNameEn = a.CheckInTime.HasValue != a.CheckOutTime.HasValue ? "Incomplete" : a.Status.NameEn,
                StatusNameAr = a.CheckInTime.HasValue != a.CheckOutTime.HasValue ? "غير مكتمل" : a.Status.NameAr,
                WorkedHours = a.WorkedHours,
                OvertimeHours = a.OvertimeHours,
                IsLate = a.IsLate
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<EmployeeBalanceSnapshotDto> GetEmployeeBalanceSnapshotAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var snapshot = new EmployeeBalanceSnapshotDto
        {
            VacationYear = now.Year,
            PermissionYear = now.Year,
            PermissionMonth = now.Month
        };

        var latestBalanceYear = await _context.EmployeeLeaveBalances
            .Where(b => b.EmployeeId == employeeId)
            .MaxAsync(b => (int?)b.Year, cancellationToken);

        if (latestBalanceYear.HasValue)
        {
            snapshot.VacationYear = latestBalanceYear.Value;
            snapshot.VacationBalances = await _context.EmployeeLeaveBalances
                .AsNoTracking()
                .Where(b => b.EmployeeId == employeeId && b.Year == latestBalanceYear.Value)
                .OrderBy(b => b.VacationType.SortOrder)
                .ThenBy(b => b.VacationType.NameEn)
                .Select(b => new EmployeeVacationBalanceSummaryDto
                {
                    VacationTypeId = b.VacationTypeId,
                    VacationTypeNameEn = b.VacationType.NameEn,
                    VacationTypeNameAr = b.VacationType.NameAr,
                    AllocatedDays = b.AllocatedDays,
                    CarryOverDays = b.CarryOverDays,
                    ManualAdjustmentDays = b.ManualAdjustmentDays,
                    UsedDays = b.UsedDays,
                    AvailableDays = b.AllocatedDays + b.CarryOverDays + b.ManualAdjustmentDays - b.UsedDays,
                    Notes = b.Notes
                })
                .ToListAsync(cancellationToken);
        }
        else
        {
            snapshot.VacationBalances = Array.Empty<EmployeeVacationBalanceSummaryDto>();
        }

        var permissionLimits = await _context.EmployeePermissionLimits
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId)
            .OrderBy(l => l.PermissionType.SortOrder)
            .ThenBy(l => l.PermissionType.NameEn)
            .Select(l => new PermissionLimitProjection(
                l.PermissionTypeId,
                l.PermissionType.NameEn,
                l.PermissionType.NameAr,
                l.MaxHoursPerMonth,
                l.Notes))
            .ToListAsync(cancellationToken);

        var permissionSummaries = new List<EmployeePermissionBalanceSummaryDto>(permissionLimits.Count);

        if (permissionLimits.Count > 0)
        {
            var startOfMonth = new DateTime(snapshot.PermissionYear, snapshot.PermissionMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var approvedStatuses = new[] { EmployeeRequestStatus.Approved, EmployeeRequestStatus.Completed };

            var usageLookup = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(d => d.EmployeeRequest.EmployeeId == employeeId
                            && d.PermissionDate >= startOfMonth
                            && d.PermissionDate < endOfMonth
                            && approvedStatuses.Contains(d.EmployeeRequest.Status))
                .GroupBy(d => d.PermissionTypeId)
                .Select(g => new
                {
                    PermissionTypeId = g.Key,
                    TotalHours = g.Sum(x => x.TotalHours)
                })
                .ToDictionaryAsync(x => x.PermissionTypeId, x => x.TotalHours, cancellationToken);

            foreach (var limit in permissionLimits)
            {
                usageLookup.TryGetValue(limit.PermissionTypeId, out var usedHours);

                var remaining = limit.MaxHoursPerMonth.HasValue
                    ? Math.Max(0, limit.MaxHoursPerMonth.Value - usedHours)
                    : (decimal?)null;

                permissionSummaries.Add(new EmployeePermissionBalanceSummaryDto
                {
                    PermissionTypeId = limit.PermissionTypeId,
                    PermissionTypeNameEn = limit.PermissionTypeNameEn,
                    PermissionTypeNameAr = limit.PermissionTypeNameAr,
                    MaxHoursPerMonth = limit.MaxHoursPerMonth,
                    UsedHoursThisMonth = usedHours,
                    RemainingHoursThisMonth = remaining,
                    Notes = limit.Notes
                });
            }
        }

        snapshot.PermissionBalances = permissionSummaries;

        return snapshot;
    }

    private sealed record PermissionLimitProjection(
        Guid PermissionTypeId,
        string PermissionTypeNameEn,
        string PermissionTypeNameAr,
        decimal? MaxHoursPerMonth,
        string? Notes);

    private async Task<List<EmployeeDocumentGroupDetailsDto>> GetEmployeeDocumentsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var rawDocuments = await _context.EmployeeDocuments
            .AsNoTracking()
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new
            {
                d.Id,
                d.DocumentName,
                d.DocumentType,
                d.FilePath,
                d.FileUrl,
                d.Description,
                d.ExpiryDate,
                d.FileSize,
                d.ContentType
            })
            .ToListAsync(cancellationToken);

        var processedDocuments = new List<DocumentCacheItem>(rawDocuments.Count);

        foreach (var doc in rawDocuments)
        {
            string? resolvedUrl = doc.FileUrl;

            if (!string.IsNullOrWhiteSpace(doc.FilePath))
            {
                resolvedUrl = await _storageService.DownloadFileUrl(doc.FilePath, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(doc.FileUrl) &&
                     !doc.FileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                resolvedUrl = await _storageService.DownloadFileUrl(doc.FileUrl, cancellationToken);
            }

            processedDocuments.Add(new DocumentCacheItem(
                doc.Id,
                doc.DocumentType,
                doc.DocumentName,
                doc.Description,
                doc.ExpiryDate,
                resolvedUrl,
                doc.FileSize,
                doc.ContentType));
        }

        var grouped = processedDocuments
            .GroupBy(d => d.DocumentType)
            .Select(g =>
            {
                var typeInfo = g.Key.GetInfo();
                return new EmployeeDocumentGroupDetailsDto
                {
                    DocumentType = g.Key,
                    DocumentTypeNameEn = typeInfo.NameEn,
                    DocumentTypeNameAr = typeInfo.NameAr,
                    Attachments = g.Select(doc => new EmployeeDocumentAttachmentDetailsDto
                    {
                        Id = doc.Id,
                        DocumentName = doc.DocumentName,
                        Description = doc.Description,
                        ExpiryDate = doc.ExpiryDate,
                        FileUrl = doc.FileUrl,
                        FileSize = doc.FileSize,
                        ContentType = doc.ContentType
                    }).ToList()
                };
            })
            .ToList();

        return grouped;
    }

    private sealed record DocumentCacheItem(
        Guid Id,
        EmployeeDocumentType DocumentType,
        string DocumentName,
        string? Description,
        DateTime? ExpiryDate,
        string? FileUrl,
        long FileSize,
        string ContentType);

    private async Task<List<EmployeeAssetDetailsDto>> GetEmployeeAssetsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var assets = await _context.EmployeeAssets
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AssignedDate)
            .ToListAsync(cancellationToken);

        var result = new List<EmployeeAssetDetailsDto>(assets.Count);
        foreach (var a in assets)
        {
            var imageUrl = !string.IsNullOrWhiteSpace(a.ImageUrl)
                ? await _storageService.DownloadFileUrl(a.ImageUrl, cancellationToken)
                : null;

            result.Add(new EmployeeAssetDetailsDto
            {
                Id = a.Id,
                AssetType = a.AssetType,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Model = a.Model,
                Description = a.Description,
                AssignedDate = a.AssignedDate,
                ExpectedReturnDate = a.ExpectedReturnDate,
                ReturnDate = a.ReturnDate,
                IsReturned = a.IsReturned,
                ReturnNotes = a.ReturnNotes,
                Condition = a.Condition,
                Value = a.Value,
                ImageUrl = imageUrl
            });
        }
        return result;
    }
}
