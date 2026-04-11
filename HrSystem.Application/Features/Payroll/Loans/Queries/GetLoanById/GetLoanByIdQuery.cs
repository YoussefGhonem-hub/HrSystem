using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Queries.GetLoanById;

public record GetLoanByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<LoanDto>>>;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, ErrorOr<GenericResponse<LoanDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetLoanByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LoanDto>>> Handle(
        GetLoanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => !l.IsDeleted && l.Id == request.Id, cancellationToken);

        if (loan == null)
        {
            return Error.NotFound("Loan.NotFound", "Loan not found");
        }

        // Verify branch scope for HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        if (!isSuperOrOrgAdmin && CurrentUser.BranchId.HasValue && loan.Employee?.BranchId != CurrentUser.BranchId)
        {
            return Error.Forbidden("Loan.BranchMismatch", "You can only view loans for employees in your branch");
        }

        // Retrieve the payroll months in which this loan was actually deducted.
        // Requires PayslipDeduction.LoanId (added in migration AddLoanIdToPayslipDeduction).
        var paidPeriods = await (
            from pd in _context.PayslipDeductions.Where(pd => pd.LoanId == request.Id)
            join p in _context.Payslips.Where(p => !p.IsDeleted) on pd.PayslipId equals p.Id
            join pc in _context.PayrollCycles on p.PayrollCycleId equals pc.Id
            select new PaidPaymentPeriodDto { Month = pc.Month, Year = pc.Year, Amount = pd.Amount }
        ).OrderBy(p => p.Year).ThenBy(p => p.Month)
         .ToListAsync(cancellationToken);

        var dto = new LoanDto
        {
            Id = loan.Id,
            EmployeeId = loan.EmployeeId,
            EmployeeCode = loan.Employee.EmployeeCode,
            EmployeeNameEn = loan.Employee.FullNameEn,
            EmployeeNameAr = loan.Employee.FullNameAr,
            LoanName = loan.LoanName,
            TotalAmount = loan.TotalAmount,
            RemainingAmount = loan.RemainingAmount,
            MonthlyDeduction = loan.MonthlyDeduction,
            InstallmentMonths = loan.InstallmentMonths,
            StartDate = loan.StartDate,
            // Compute EndDate from StartDate + InstallmentMonths when not explicitly stored
            EndDate = loan.EndDate ?? loan.StartDate.AddMonths(loan.InstallmentMonths),
            IsActive = loan.IsActive,
            Notes = loan.Notes,
            PaidPaymentPeriods = paidPeriods
        };

        return new GenericResponse<LoanDto>
        {
            Success = true,
            Message = "Loan retrieved successfully",
            Data = dto
        };
    }
}
