using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Queries.GetPendingDisbursements;

/// <summary>
/// A loan that is active and whose deductions haven't started yet
/// (no PayslipDeduction record with this LoanId exists).
/// Shown as a pre-generate warning in the payslip generation dialog.
/// </summary>
public class PendingDisbursementDto
{
    public Guid LoanId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string LoanName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal MonthlyDeduction { get; set; }
    public DateTime StartDate { get; set; }
}

/// <summary>
/// Returns loans that are:
///   1. Active and not deleted
///   2. StartDate is on or before the last day of the requested month/year
///      (i.e. they should have started by this payroll cycle)
///   3. Have NO PayslipDeduction record linked to them yet
///      (meaning the first deduction hasn't been applied yet)
/// </summary>
public record GetPendingDisbursementsQuery(int Month, int Year)
    : IRequest<ErrorOr<GenericResponse<List<PendingDisbursementDto>>>>;

public class GetPendingDisbursementsQueryHandler
    : IRequestHandler<GetPendingDisbursementsQuery, ErrorOr<GenericResponse<List<PendingDisbursementDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPendingDisbursementsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<PendingDisbursementDto>>>> Handle(
        GetPendingDisbursementsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");

        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        // Resolve the actual pay period(s) of the month from the organization's payroll settings.
        var settings = await PayrollPeriodCalculator.LoadSettingsAsync(
            _context, CurrentUser.OrganizationId ?? Guid.Empty, cancellationToken);
        var periods = PayrollPeriodCalculator.GetPeriodsForMonth(settings, request.Month, request.Year);
        var periodStart = periods.Min(p => p.StartDate);
        var periodEnd = periods.Max(p => p.EndDate);

        // Apply branch scope for HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Collect loan IDs that already have at least one deduction on a paid payslip.
        // Unpaid payslips should not advance loan payment status.
        var disbursedLoanIds = await (
            from pd in _context.PayslipDeductions
            where pd.LoanId.HasValue
            join p in _context.Payslips.Where(p => !p.IsDeleted && p.IsPaid)
                on pd.PayslipId equals p.Id
            select pd.LoanId!.Value
        )
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _context.Loans
            .Include(l => l.Employee)
            .Where(l => !l.IsDeleted
                        && l.IsActive
                        && l.StartDate <= periodEnd
                        && l.RemainingAmount > 0
                        // Exclude loans whose last installment month is before the requested period.
                        // Effective end = explicit EndDate, else StartDate + (InstallmentMonths - 1).
                        && (l.EndDate ?? l.StartDate.AddMonths(l.InstallmentMonths > 0 ? l.InstallmentMonths - 1 : 0)) >= periodStart
                        && !disbursedLoanIds.Contains(l.Id));

        if (branchId.HasValue)
            query = query.Where(l => l.Employee.BranchId == branchId.Value);

        var loans = await query
            .OrderBy(l => l.Employee.EmployeeCode)
            .Select(l => new PendingDisbursementDto
            {
                LoanId = l.Id,
                EmployeeCode = l.Employee.EmployeeCode,
                EmployeeNameEn = l.Employee.FullNameEn,
                EmployeeNameAr = l.Employee.FullNameAr,
                LoanName = l.LoanName,
                TotalAmount = l.TotalAmount,
                MonthlyDeduction = l.MonthlyDeduction,
                StartDate = l.StartDate
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<PendingDisbursementDto>>
        {
            Success = true,
            Message = $"{loans.Count} pending loan disbursement(s) found for {request.Month}/{request.Year}.",
            Data = loans
        };
    }
}
