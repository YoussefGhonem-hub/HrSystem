using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeFullCommandValidator : AbstractValidator<CreateEmployeeFullCommand>
{
    public CreateEmployeeFullCommandValidator(ApplicationDbContext context)
    {
        RuleFor(x => x.PersonalInfo)
            .NotNull()
            .SetValidator(new CreateEmployeePersonalInfoSectionValidator(context));

        When(x => x.JobInfo is not null, () =>
        {
            RuleFor(x => x.JobInfo!)
                .SetValidator(new CreateEmployeeJobInfoSectionValidator(context));
        });

        When(x => x.Payroll is not null, () =>
        {
            RuleFor(x => x.Payroll!)
                .SetValidator(new CreateEmployeePayrollSectionValidator());
        });

        When(x => x.Attendance is not null, () =>
        {
            RuleFor(x => x.Attendance!)
                .SetValidator(new CreateEmployeeAttendanceSectionValidator());
        });
        
        When(x => x.Documents is not null, () =>
        {
            RuleFor(x => x.Documents!)
                .SetValidator(new CreateEmployeeDocumentsSectionValidator());
        });

        When(x => x.Assets is not null, () =>
        {
            RuleFor(x => x.Assets!)
                .SetValidator(new CreateEmployeeAssetsSectionValidator());
        });
    }
}

public class CreateEmployeePersonalInfoSectionValidator : AbstractValidator<CreateEmployeePersonalInfoSection>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeePersonalInfoSectionValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FirstNameAr)
            .NotEmpty().WithMessage("First name in Arabic is required")
            .MaximumLength(100);

        RuleFor(x => x.LastNameAr)
            .NotEmpty().WithMessage("Last name in Arabic is required")
            .MaximumLength(100);

        RuleFor(x => x.FirstNameEn)
            .NotEmpty().WithMessage("First name in English is required")
            .MaximumLength(100);

        RuleFor(x => x.LastNameEn)
            .NotEmpty().WithMessage("Last name in English is required")
            .MaximumLength(100);

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(BeUniqueNationalId).WithMessage("National ID already exists");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.AddressAr)
            .NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .Must(BeValidAge).WithMessage("Employee must be at least 18 years old");
    }

    private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(e => e.NationalId == nationalId, cancellationToken);
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(e => e.Email == email, cancellationToken);
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }
}

public class CreateEmployeeJobInfoSectionValidator : AbstractValidator<CreateEmployeeJobInfoSection>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeJobInfoSectionValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");

        RuleFor(x => x.JobTitleId)
            .NotEmpty()
            .MustAsync(JobTitleExists).WithMessage("Job title does not exist");

        RuleFor(x => x.DirectManagerId)
            .MustAsync(ManagerExists).WithMessage("Direct manager does not exist")
            .When(x => x.DirectManagerId.HasValue);

        RuleFor(x => x.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.ContractTypeId)
            .NotEmpty();

        RuleFor(x => x.HiringDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hiring date cannot be in the future");

        RuleFor(x => x.ProbationPeriodMonths)
            .InclusiveBetween(0, 12);
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
    }

    private async Task<bool> JobTitleExists(Guid jobTitleId, CancellationToken cancellationToken)
    {
        return await _context.JobTitles.AnyAsync(j => j.Id == jobTitleId, cancellationToken);
    }

    private async Task<bool> ManagerExists(Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == managerId.Value, cancellationToken);
    }

    private async Task<bool> BranchExists(Guid? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue) return true;
        return await _context.Branches.AnyAsync(b => b.Id == branchId.Value, cancellationToken);
    }
}

public class CreateEmployeePayrollSectionValidator : AbstractValidator<CreateEmployeePayrollSection>
{
    public CreateEmployeePayrollSectionValidator()
    {
        RuleFor(x => x.BasicSalary)
            .GreaterThanOrEqualTo(0m);

        RuleFor(x => x.EffectiveDate)
            .NotEmpty();

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod));

        RuleFor(x => x.SocialInsuranceEmployeeRate)
            .InclusiveBetween(0, 100)
            .When(x => x.SocialInsuranceEmployeeRate.HasValue);

        RuleFor(x => x.SocialInsuranceEmployerRate)
            .InclusiveBetween(0, 100)
            .When(x => x.SocialInsuranceEmployerRate.HasValue);

        When(x => x.BankInfo is not null, () =>
        {
            RuleFor(x => x.BankInfo!)
                .SetValidator(new PayrollBankInfoPayloadValidator());
        });

        RuleForEach(x => x.Allowances)
            .SetValidator(new PayrollAllowancePayloadValidator());

        RuleForEach(x => x.Deductions)
            .SetValidator(new PayrollDeductionPayloadValidator());
    }
}

public class CreateEmployeeAttendanceSectionValidator : AbstractValidator<CreateEmployeeAttendanceSection>
{
    public CreateEmployeeAttendanceSectionValidator()
    {
        RuleFor(x => x.WorkShift).MaximumLength(128);
        RuleFor(x => x.WorkDays).MaximumLength(128);
        RuleFor(x => x.GracePeriod).MaximumLength(64);
        RuleFor(x => x.MaxLatePerMonth).MaximumLength(64);
        RuleFor(x => x.AttendanceMethod).MaximumLength(128);
        RuleFor(x => x.LateDeductionPolicy).MaximumLength(128);
        RuleFor(x => x.AbsenceDeductionPolicy).MaximumLength(128);
        RuleFor(x => x.HalfDayRule).MaximumLength(128);
        RuleFor(x => x.MissingCheckoutHandling).MaximumLength(128);
    }
}

public class CreateEmployeeDocumentsSectionValidator : AbstractValidator<CreateEmployeeDocumentsSection>
{
    public CreateEmployeeDocumentsSectionValidator()
    {
        RuleFor(x => x.Types)
            .NotEmpty();

        RuleForEach(x => x.Types)
            .SetValidator(new CreateEmployeeDocumentTypeGroupValidator());
    }
}

public class CreateEmployeeDocumentTypeGroupValidator : AbstractValidator<CreateEmployeeDocumentTypeGroup>
{
    public CreateEmployeeDocumentTypeGroupValidator()
    {
        RuleFor(x => x.Attachments)
            .NotEmpty();

        RuleForEach(x => x.Attachments)
            .Must(file => file != null && file.Length > 0)
            .WithMessage("Document file must not be empty");
    }
}

public class CreateEmployeeAssetsSectionValidator : AbstractValidator<CreateEmployeeAssetsSection>
{
    public CreateEmployeeAssetsSectionValidator()
    {
        RuleForEach(x => x.Assets)
            .SetValidator(new CreateEmployeeAssetPayloadValidator());
    }
}

public class CreateEmployeeAssetPayloadValidator : AbstractValidator<CreateEmployeeAssetPayload>
{
    public CreateEmployeeAssetPayloadValidator()
    {
        RuleFor(x => x.AssetType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AssetName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Condition)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AssignedDate)
            .NotEmpty();

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Value.HasValue);
    }
}
