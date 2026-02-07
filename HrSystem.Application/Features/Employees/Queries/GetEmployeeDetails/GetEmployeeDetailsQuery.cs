using ErrorOr;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
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

        var payroll = await GetEmployeePayrollDetailsAsync(request.EmployeeId, cancellationToken);
        var payrollHistory = await GetPayrollHistoryAsync(request.EmployeeId, cancellationToken);
        var attendance = await GetAttendanceConfigurationAsync(request.EmployeeId, cancellationToken);
        var attendanceHistory = await GetAttendanceHistoryAsync(request.EmployeeId, request.AttendanceRecentCount, cancellationToken);
        var documents = await GetEmployeeDocumentsAsync(request.EmployeeId, cancellationToken);
        var assets = await GetEmployeeAssetsAsync(request.EmployeeId, cancellationToken);

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
            JobInfo = MapJobInfo(employee),
            Payroll = payroll,
            Attendance = attendance,
            Documents = documents,
            Assets = assets
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

    private static EmployeeJobInfoDetailsDto MapJobInfo(HrSystem.Domain.Entities.Employee.Employee employee)
    {
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
            ProbationPeriodMonths = employee.ProbationPeriodMonths
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

        var dto = new EmployeePayrollDetailsDto
        {
            BasicSalary = salary.BasicSalary,
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
            Allowances = salary.Allowances
                .Select(a => new PayrollAllowancePayload(a.NameAr, a.NameEn, a.Description, a.IsTaxable, a.IsSubjectToInsurance, a.Amount, a.IsPercentage, a.PercentageValue))
                .ToList(),
            Deductions = salary.Deductions
                .Select(d => new PayrollDeductionPayload(d.NameAr, d.NameEn, d.Description, d.IsRecurring, d.Amount, d.IsPercentage, d.PercentageValue))
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

        return new PayrollBankInfoPayload(bankName, bankBranch, accountNumber, iban, swiftCode);
    }

    private async Task<EmployeeAttendanceDetailsDto?> GetAttendanceConfigurationAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var configuration = await _context.Attendances
            .AsNoTracking()
            .IgnoreQueryFilters()
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
                StatusNameEn = a.Status.NameEn,
                StatusNameAr = a.Status.NameAr,
                WorkedHours = a.WorkedHours,
                OvertimeHours = a.OvertimeHours,
                IsLate = a.IsLate
            })
            .ToListAsync(cancellationToken);
    }

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
        return await _context.EmployeeAssets
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AssignedDate)
            .Select(a => new EmployeeAssetDetailsDto
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
                Value = a.Value
            })
            .ToListAsync(cancellationToken);
    }
}
