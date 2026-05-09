using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand(
    Guid SubscriptionPlanId,
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

public class UpdateSubscriptionPlanCommandHandler
    : IRequestHandler<UpdateSubscriptionPlanCommand, ErrorOr<GenericResponse<Guid>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateSubscriptionPlanCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<Guid>>> Handle(
        UpdateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request);
        if (!string.IsNullOrWhiteSpace(validationError))
        {
            return Error.Validation("SubscriptionPlan.Validation", validationError);
        }

        var plan = await _context.SubscriptionPlans
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId && !p.IsDeleted, cancellationToken);

        if (plan is null)
        {
            return Error.NotFound(
                code: "SubscriptionPlan.NotFound",
                description: "Subscription plan not found.");
        }

        var normalizedCode = NormalizeCode(request.Code);

        var duplicateCodeExists = await _context.SubscriptionPlans
            .IgnoreQueryFilters()
            .AnyAsync(
                p => !p.IsDeleted && p.Id != request.SubscriptionPlanId && p.Code.ToUpper() == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Error.Conflict(
                code: "SubscriptionPlan.CodeAlreadyExists",
                description: "A subscription plan with the same code already exists.");
        }

        plan.Code = normalizedCode;
        plan.NameEn = request.NameEn.Trim();
        plan.NameAr = request.NameAr.Trim();
        plan.DescriptionEn = NormalizeNullable(request.DescriptionEn);
        plan.DescriptionAr = NormalizeNullable(request.DescriptionAr);
        plan.MonthlyPrice = decimal.Round(request.MonthlyPrice, 2, MidpointRounding.AwayFromZero);
        plan.AnnualPrice = decimal.Round(request.AnnualPrice, 2, MidpointRounding.AwayFromZero);
        plan.Currency = request.Currency.Trim().ToUpperInvariant();
        plan.MaxEmployees = request.MaxEmployees;
        plan.MaxStorageGB = request.MaxStorageGB;
        plan.MaxDepartments = request.MaxDepartments;
        plan.AllowBiometricIntegration = request.AllowBiometricIntegration;
        plan.AllowPayrollModule = request.AllowPayrollModule;
        plan.AllowPerformanceModule = request.AllowPerformanceModule;
        plan.AllowRecruitmentModule = request.AllowRecruitmentModule;
        plan.AllowCustomReports = request.AllowCustomReports;
        plan.AllowAPIAccess = request.AllowAPIAccess;
        plan.TrialDays = request.TrialDays;
        plan.IsActive = request.IsActive;
        plan.DisplayOrder = request.DisplayOrder;
        plan.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<Guid>.SuccessResult(
            plan.Id,
            "Subscription plan updated successfully.");
    }

    private static string? ValidateRequest(UpdateSubscriptionPlanCommand request)
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
