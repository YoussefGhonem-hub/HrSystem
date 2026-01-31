using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Lifecycle;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;
using System.IO;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeFullCommandHandler : IRequestHandler<CreateEmployeeFullCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly IStorageService _storageService;

    public CreateEmployeeFullCommandHandler(ApplicationDbContext context, ISender sender, IStorageService storageService)
    {
        _context = context;
        _sender = sender;
        _storageService = storageService;
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
            TenantId = Guid.NewGuid()
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
        var payrollCommand = new ConfigureEmployeePayrollCommand(
            employee.Id,
            payroll.BasicSalary,
            payroll.EffectiveDate,
            payroll.Currency,
            payroll.IncludeSocialInsurance,
            payroll.SocialInsuranceEmployeeRate,
            payroll.SocialInsuranceEmployerRate,
            payroll.PaymentMethod,
            payroll.BankInfo,
            payroll.Allowances,
            payroll.Deductions,
            payroll.Notes
        );

        var result = await _sender.Send(payrollCommand, cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

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
}
