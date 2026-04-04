using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Lifecycle;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;
using System.IO;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeFullCommandHandler : IRequestHandler<CreateEmployeeFullCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateEmployeeFullCommandHandler(
        ApplicationDbContext context,
        ISender sender,
        IStorageService storageService,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _storageService = storageService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        CreateEmployeeFullCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var employee = await CreateEmployeeAsync(request.PersonalInfo, request.JobInfo, cancellationToken);

        await ConfigureAttendanceProfileAsync(employee, request.Attendance, request.JobInfo.HiringDate, cancellationToken);

        var assetsResult = await AssignEmployeeAssetsAsync(employee, request.Assets, cancellationToken);
        if (assetsResult.IsError)
        {
            await transaction.RollbackAsync(cancellationToken);
            return assetsResult.Errors;
        }

        var documentsResult = await UploadEmployeeDocumentsAsync(employee, request.Documents, cancellationToken);
        if (documentsResult.IsError)
        {
            await transaction.RollbackAsync(cancellationToken);
            return documentsResult.Errors;
        }

        var payrollResult = await ConfigureEmployeePayrollAsync(employee, request.Payroll, cancellationToken);
        if (payrollResult.IsError)
        {
            await transaction.RollbackAsync(cancellationToken);
            return payrollResult.Errors;
        }

        // Create leave balances using form data or defaults
        await CreateLeaveBalancesAsync(employee, request.Leaves, cancellationToken);

        // Create user account and assign role if RoleId provided
        if (request.JobInfo.RoleId.HasValue && request.JobInfo.RoleId.Value != Guid.Empty)
        {
            var userResult = await CreateUserAccountAsync(employee, request.PersonalInfo, request.JobInfo, cancellationToken);
            if (userResult.IsError)
            {
                await transaction.RollbackAsync(cancellationToken);
                return userResult.Errors;
            }
        }

        await transaction.CommitAsync(cancellationToken);

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee created with payroll configuration",
            Data = dto
        };
    }

    private async Task<Employee> CreateEmployeeAsync(
        CreateEmployeePersonalInfoSection personal,
        CreateEmployeeJobInfoSection job,
        CancellationToken cancellationToken)
    {
        var employeeCode = await EmployeeCommandHelper.GenerateEmployeeCodeAsync(_context, cancellationToken);

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FirstNameAr = personal.FirstNameAr,
            LastNameAr = personal.LastNameAr,
            FirstNameEn = personal.FirstNameEn,
            LastNameEn = personal.LastNameEn,
            NationalId = personal.NationalId,
            PassportNumber = personal.PassportNumber,
            DateOfBirth = personal.DateOfBirth,
            GenderId = personal.GenderId,
            MaritalStatusId = personal.MaritalStatusId,
            Email = personal.Email,
            PhoneNumber = personal.PhoneNumber,
            MobileNumber = personal.MobileNumber,
            AddressAr = personal.AddressAr,
            AddressEn = personal.AddressEn,
            City = personal.City,
            Country = personal.Country,
            StatusId = EmployeeStatusIds.Probation,
            TenantId = Guid.Empty
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        employee.DepartmentId = job.DepartmentId;
        employee.JobTitleId = job.JobTitleId;
        employee.DirectManagerId = job.DirectManagerId;
        employee.BranchId = job.BranchId;
        employee.ContractTypeId = job.ContractTypeId;
        employee.HiringDate = job.HiringDate;
        employee.ProbationPeriodMonths = job.ProbationPeriodMonths;
        employee.ProbationEndDate = job.HiringDate.AddMonths(job.ProbationPeriodMonths);
        employee.StatusId = EmployeeStatusIds.Active;

        await _context.SaveChangesAsync(cancellationToken);

        return employee;
    }

    private async Task ConfigureAttendanceProfileAsync(
        Employee employee,
        CreateEmployeeAttendanceSection? attendanceSection,
        DateTime hiringDate,
        CancellationToken cancellationToken)
    {
        if (attendanceSection is null)
        {
            return;
        }

        var attendanceProfile = new HrSystem.Domain.Entities.Attendance.Attendance
        {
            EmployeeId = employee.Id,
            Date = hiringDate.Date,
            StatusId = AttendanceStatusIds.Present,
            IsConfigurationRecord = true,
            WorkShift = attendanceSection.WorkShift,
            WorkDays = attendanceSection.WorkDays,
            GracePeriod = attendanceSection.GracePeriod,
            MaxLatePerMonth = attendanceSection.MaxLatePerMonth,
            OvertimeEligible = attendanceSection.OvertimeEligible,
            AttendanceMethod = attendanceSection.AttendanceMethod,
            LateDeductionPolicy = attendanceSection.LateDeductionPolicy,
            AbsenceDeductionPolicy = attendanceSection.AbsenceDeductionPolicy,
            HalfDayRule = attendanceSection.HalfDayRule,
            MissingCheckoutHandling = attendanceSection.MissingCheckoutHandling
        };

        _context.Attendances.Add(attendanceProfile);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ErrorOr<Unit>> UploadEmployeeDocumentsAsync(
        Employee employee,
        CreateEmployeeDocumentsSection? documentsSection,
        CancellationToken cancellationToken)
    {
        if (documentsSection is not { Types.Count: > 0 })
        {
            return Unit.Value;
        }

        var documentEntities = new List<EmployeeDocument>();

        foreach (var group in documentsSection.Types)
        {
            if (group.Attachments == null || group.Attachments.Count == 0)
            {
                continue;
            }

            var typeInfo = group.DocumentType.GetInfo();

            foreach (var attachment in group.Attachments)
            {
                if (attachment == null || attachment.Length == 0)
                {
                    continue;
                }

                var stored = await _storageService.Upload(attachment, cancellationToken);
                if (stored == null || string.IsNullOrWhiteSpace(stored.Key))
                {
                    return Error.Failure("EmployeeDocuments.UploadFailed", "Failed to upload employee documents to storage");
                }

                var fileUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);
                var documentName = string.IsNullOrWhiteSpace(attachment.FileName)
                    ? typeInfo.NameEn
                    : Path.GetFileName(attachment.FileName);

                documentEntities.Add(new EmployeeDocument
                {
                    EmployeeId = employee.Id,
                    DocumentType = group.DocumentType,
                    DocumentName = documentName,
                    FilePath = stored.Key,
                    FileUrl = fileUrl,
                    Description = null,
                    ExpiryDate = null,
                    FileSize = attachment.Length,
                    ContentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                        ? "application/octet-stream"
                        : attachment.ContentType,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId
                });
            }
        }

        if (documentEntities.Count == 0)
        {
            return Unit.Value;
        }

        _context.EmployeeDocuments.AddRange(documentEntities);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task<ErrorOr<Unit>> ConfigureEmployeePayrollAsync(
        Employee employee,
        CreateEmployeePayrollSection payroll,
        CancellationToken cancellationToken)
    {
        // Configure salary directly instead of dispatching via MediatR
        // to avoid the payroll handler re-querying the employee (which can fail
        // within the same transaction when CreateEmployee just inserted it).
        var tenantId = employee.TenantId != Guid.Empty
            ? employee.TenantId
            : Guid.NewGuid();

        var branchId = employee.BranchId;

        var currency = !string.IsNullOrWhiteSpace(payroll.Currency)
            ? payroll.Currency.Trim().ToUpperInvariant()
            : "EGP";

        var salary = new Domain.Entities.Payroll.Salary
        {
            EmployeeId = employee.Id,
            BasicSalary = payroll.BasicSalary,
            EffectiveDate = payroll.EffectiveDate,
            Notes = payroll.Notes,
            IsCurrent = true,
            Currency = currency,
            IsSocialInsuranceEnabled = payroll.IncludeSocialInsurance,
            SocialInsuranceEmployeeRate = payroll.SocialInsuranceEmployeeRate,
            SocialInsuranceEmployerRate = payroll.SocialInsuranceEmployerRate,
            PaymentMethod = payroll.PaymentMethod,
            BankName = payroll.BankInfo?.BankName,
            BankBranch = payroll.BankInfo?.BankBranch,
            BankAccountNumber = payroll.BankInfo?.AccountNumber,
            BankIban = payroll.BankInfo?.Iban,
            BankSwiftCode = payroll.BankInfo?.SwiftCode,
            TenantId = tenantId,
            BranchId = branchId
        };

        _context.Salaries.Add(salary);

        if (payroll.Allowances is { Count: > 0 })
        {
            foreach (var a in payroll.Allowances)
            {
                salary.Allowances.Add(new Domain.Entities.Payroll.SalaryAllowance
                {
                    SalaryId = salary.Id,
                    NameAr = a.NameAr,
                    NameEn = a.NameEn,
                    Description = a.Description,
                    IsTaxable = a.IsTaxable,
                    IsSubjectToInsurance = a.IsSubjectToInsurance,
                    Amount = a.Amount,
                    IsPercentage = a.IsPercentage,
                    PercentageValue = a.PercentageValue
                });
            }
        }

        if (payroll.Deductions is { Count: > 0 })
        {
            foreach (var d in payroll.Deductions)
            {
                salary.Deductions.Add(new Domain.Entities.Payroll.SalaryDeduction
                {
                    SalaryId = salary.Id,
                    NameAr = d.NameAr,
                    NameEn = d.NameEn,
                    Description = d.Description,
                    IsRecurring = d.IsRecurring,
                    Amount = d.Amount,
                    IsPercentage = d.IsPercentage,
                    PercentageValue = d.PercentageValue
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<ErrorOr<Unit>> AssignEmployeeAssetsAsync(
        Employee employee,
        CreateEmployeeAssetsSection? assetsSection,
        CancellationToken cancellationToken)
    {
        if (assetsSection is not { Assets.Count: > 0 })
        {
            return Unit.Value;
        }

        var assetEntities = new List<EmployeeAsset>();

        foreach (var asset in assetsSection.Assets)
        {
            if (asset == null)
            {
                continue;
            }

            assetEntities.Add(new EmployeeAsset
            {
                EmployeeId = employee.Id,
                AssetType = asset.AssetType,
                AssetName = asset.AssetName,
                SerialNumber = asset.SerialNumber,
                Model = asset.Model,
                Description = asset.Description,
                AssignedDate = asset.AssignedDate,
                ExpectedReturnDate = asset.ExpectedReturnDate,
                ReturnDate = asset.ReturnDate,
                IsReturned = asset.IsReturned,
                ReturnNotes = asset.ReturnNotes,
                Condition = asset.Condition,
                Value = asset.Value,
                TenantId = employee.TenantId
            });
        }

        if (assetEntities.Count == 0)
        {
            return Unit.Value;
        }

        _context.EmployeeAssets.AddRange(assetEntities);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task CreateLeaveBalancesAsync(Employee employee, CreateEmployeeLeavesSection? leaves, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;

        var vacationTypes = await _context.VacationTypes
            .AsNoTracking()
            .Where(v => v.IsActive)
            .Select(v => new { v.Id, v.NameEn })
            .ToListAsync(cancellationToken);

        if (vacationTypes.Count == 0) return;

        // Use form values if provided, otherwise fall back to defaults
        var allocations = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Annual Leave"] = leaves?.Annual ?? 21m,
            ["Sick Leave"] = leaves?.Sick ?? 14m,
            ["Emergency Leave"] = leaves?.Emergency ?? 7m,
            ["Compensatory Leave"] = leaves?.Compensatory ?? 0m,
            ["Unpaid Leave"] = 0m
        };

        foreach (var vt in vacationTypes)
        {
            var allocatedDays = allocations.TryGetValue(vt.NameEn, out var days) ? days : 0m;

            _context.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
            {
                EmployeeId = employee.Id,
                VacationTypeId = vt.Id,
                Year = currentYear,
                AllocatedDays = allocatedDays,
                CarryOverDays = 0m,
                ManualAdjustmentDays = 0m,
                UsedDays = 0m,
                TenantId = employee.TenantId,
                BranchId = employee.BranchId,
                Notes = "Initial allocation"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ErrorOr<Unit>> CreateUserAccountAsync(
        Employee employee,
        CreateEmployeePersonalInfoSection personal,
        CreateEmployeeJobInfoSection job,
        CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(job.RoleId!.Value.ToString());
        if (role == null)
        {
            return Error.NotFound("Role.NotFound", "The specified role was not found");
        }

        // Check if email already used by another user
        var existingUser = await _userManager.FindByEmailAsync(personal.Email);
        if (existingUser != null)
        {
            return Error.Conflict("User.EmailExists", $"A user account with email '{personal.Email}' already exists");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = personal.Email,
            Email = personal.Email,
            EmailConfirmed = true,
            FullName = $"{personal.FirstNameEn} {personal.LastNameEn}".Trim(),
            IsActive = true,
            OrganizationId = employee.TenantId,
            BranchId = employee.BranchId,
            EmployeeId = employee.Id,
            CreatedDate = DateTimeOffset.UtcNow
        };

        // Default password: Hr@ + first 6 chars of NationalId + Xx1
        var defaultPassword = $"Hr@{personal.NationalId[..Math.Min(6, personal.NationalId.Length)]}Xx1";
        var createResult = await _userManager.CreateAsync(user, defaultPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Error.Validation("User.CreateFailed", errors);
        }

        // Add to ASP.NET Identity role
        var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!roleResult.Succeeded)
        {
            return Error.Validation("User.RoleAssignFailed", string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        // Add UserBranchRole record
        var branchId = employee.BranchId ?? job.BranchId;
        if (branchId.HasValue)
        {
            _context.UserBranchRoles.Add(new UserBranchRole
            {
                UserId = user.Id,
                BranchId = branchId.Value,
                RoleName = role.Name!
            });
        }

        // Link employee to user
        employee.UserId = user.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
