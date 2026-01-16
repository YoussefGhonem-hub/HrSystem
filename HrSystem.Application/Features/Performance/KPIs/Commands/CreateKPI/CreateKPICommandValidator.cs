using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.CreateKPI;

public class CreateKPICommandValidator : AbstractValidator<CreateKPICommand>
{
    private readonly ApplicationDbContext _context;

    public CreateKPICommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("KPI name in Arabic is required")
            .MaximumLength(200).WithMessage("KPI name must not exceed 200 characters");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("KPI name in English is required")
            .MaximumLength(200).WithMessage("KPI name must not exceed 200 characters");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DescriptionAr));

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DescriptionEn));

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Weight)
            .InclusiveBetween(1, 100).WithMessage("Weight must be between 1 and 100");

        RuleFor(x => x.MeasurementCriteria)
            .NotEmpty().WithMessage("Measurement criteria is required")
            .MaximumLength(500).WithMessage("Measurement criteria must not exceed 500 characters");

        RuleFor(x => x.JobTitleId)
            .MustAsync(JobTitleExists).WithMessage("Job title does not exist")
            .When(x => x.JobTitleId.HasValue);

        RuleFor(x => x.DepartmentId)
            .MustAsync(DepartmentExists).WithMessage("Department does not exist")
            .When(x => x.DepartmentId.HasValue);
    }

    private async Task<bool> JobTitleExists(Guid? jobTitleId, CancellationToken cancellationToken)
    {
        if (!jobTitleId.HasValue) return true;
        return await _context.JobTitles.AnyAsync(j => j.Id == jobTitleId.Value, cancellationToken);
    }

    private async Task<bool> DepartmentExists(Guid? departmentId, CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue) return true;
        return await _context.Departments.AnyAsync(d => d.Id == departmentId.Value, cancellationToken);
    }
}
