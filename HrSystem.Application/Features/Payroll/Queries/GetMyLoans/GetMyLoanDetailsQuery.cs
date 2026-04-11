using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyLoans;

/// <summary>
/// Query to get loan details for the currently logged-in user
/// </summary>
public record GetMyLoanDetailsQuery(Guid LoanId) : IRequest<ErrorOr<GenericResponse<MyLoanDetailsDto>>>;

public class GetMyLoanDetailsQueryHandler : IRequestHandler<GetMyLoanDetailsQuery, ErrorOr<GenericResponse<MyLoanDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyLoanDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyLoanDetailsDto>>> Handle(
        GetMyLoanDetailsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Loan.Unauthorized", "Current user is not linked to an employee");
        }

        // Load as entity so we can run a second query for paid periods
        var loanEntity = await _context.Loans
            .Where(l => !l.IsDeleted && l.EmployeeId == employeeId.Value && l.Id == request.LoanId)
            .FirstOrDefaultAsync(cancellationToken);

        if (loanEntity == null)
        {
            return Error.NotFound("Loan.NotFound", "Loan not found");
        }

        // Retrieve the payroll months in which this loan was actually deducted.
        var paidPeriods = await (
            from pd in _context.PayslipDeductions.Where(pd => pd.LoanId == request.LoanId)
            join p in _context.Payslips.Where(p => !p.IsDeleted) on pd.PayslipId equals p.Id
            join pc in _context.PayrollCycles on p.PayrollCycleId equals pc.Id
            select new PaidPaymentPeriodDto { Month = pc.Month, Year = pc.Year, Amount = pd.Amount }
        ).OrderBy(p => p.Year).ThenBy(p => p.Month)
         .ToListAsync(cancellationToken);

        var loan = new MyLoanDetailsDto
        {
            LoanId = loanEntity.Id,
            LoanName = loanEntity.LoanName,
            TotalAmount = loanEntity.TotalAmount,
            RemainingAmount = loanEntity.RemainingAmount,
            MonthlyDeduction = loanEntity.MonthlyDeduction,
            InstallmentMonths = loanEntity.InstallmentMonths,
            IsActive = loanEntity.IsActive,
            StartDate = loanEntity.StartDate,
            EndDate = loanEntity.EndDate ?? loanEntity.StartDate.AddMonths(loanEntity.InstallmentMonths),
            Notes = loanEntity.Notes,
            PaidPaymentPeriods = paidPeriods
        };

        return new GenericResponse<MyLoanDetailsDto>
        {
            Success = true,
            Message = "Loan details retrieved successfully",
            Data = loan
        };
    }
}
