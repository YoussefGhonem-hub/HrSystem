using ErrorOr;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand(
    string Code,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    string Currency,
    int MaxEmployees,
    int MaxStorageGB,
    int MaxDepartments,
    bool AllowBiometricIntegration,
    bool AllowPayrollModule,
    bool AllowPerformanceModule,
    bool AllowRecruitmentModule,
    bool AllowCustomReports,
    bool AllowAPIAccess,
    int TrialDays,
    bool IsActive,
    int DisplayOrder
) : IRequest<ErrorOr<GenericResponse<Guid>>>;

public class CreateSubscriptionPlanCommandHandler
    : IRequestHandler<CreateSubscriptionPlanCommand, ErrorOr<GenericResponse<Guid>>>
{
    private readonly ApplicationDbContext _context;

    public CreateSubscriptionPlanCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<Guid>>> Handle(
        CreateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request);
        if (!string.IsNullOrWhiteSpace(validationError))
        {
            return Error.Validation("SubscriptionPlan.Validation", validationError);
        }

        var normalizedCode = NormalizeCode(request.Code);

        var duplicateCodeExists = await _context.SubscriptionPlans
            .IgnoreQueryFilters()
            .AnyAsync(
                p => !p.IsDeleted && p.Code.ToUpper() == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Error.Conflict(
                code: "SubscriptionPlan.CodeAlreadyExists",
                description: "A subscription plan with the same code already exists.");
        }

        var plan = new SubscriptionPlan
        {
            Code = normalizedCode,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            DescriptionEn = NormalizeNullable(request.DescriptionEn),
            DescriptionAr = NormalizeNullable(request.DescriptionAr),
            MonthlyPrice = decimal.Round(request.MonthlyPrice, 2, MidpointRounding.AwayFromZero),
            AnnualPrice = decimal.Round(request.AnnualPrice, 2, MidpointRounding.AwayFromZero),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            MaxEmployees = request.MaxEmployees,
            MaxStorageGB = request.MaxStorageGB,
            MaxDepartments = request.MaxDepartments,
            AllowBiometricIntegration = request.AllowBiometricIntegration,
            AllowPayrollModule = request.AllowPayrollModule,
            AllowPerformanceModule = request.AllowPerformanceModule,
            AllowRecruitmentModule = request.AllowRecruitmentModule,
            AllowCustomReports = request.AllowCustomReports,
            AllowAPIAccess = request.AllowAPIAccess,
            TrialDays = request.TrialDays,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            TenantId = Guid.Empty
        };

        await _context.SubscriptionPlans.AddAsync(plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<Guid>.SuccessResult(
            plan.Id,
            "Subscription plan created successfully.");
    }

    private static string? ValidateRequest(CreateSubscriptionPlanCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return "Plan code is required.";
        }

        if (string.IsNullOrWhiteSpace(request.NameEn) || string.IsNullOrWhiteSpace(request.NameAr))
        {
            return "Plan names in English and Arabic are required.";
        }

        if (request.MonthlyPrice < 0 || request.AnnualPrice < 0)
        {
            return "Plan prices cannot be negative.";
        }

        if (request.MaxEmployees <= 0 || request.MaxStorageGB <= 0 || request.MaxDepartments <= 0)
        {
            return "Max employees, storage, and departments must be greater than zero.";
        }

        if (request.TrialDays < 0 || request.DisplayOrder < 0)
        {
            return "Trial days and display order cannot be negative.";
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return "Currency is required.";
        }

        return null;
    }

    private static string NormalizeCode(string rawCode)
    {
        var cleaned = new string(rawCode.Trim().Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? string.Empty : cleaned.ToUpperInvariant();
    }

    private static string? NormalizeNullable(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
