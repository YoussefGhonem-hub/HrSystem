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

        var loan = await _context.Loans
            .Where(l => !l.IsDeleted && l.EmployeeId == employeeId.Value && l.Id == request.LoanId)
            .Select(l => new MyLoanDetailsDto
            {
                LoanId = l.Id,
                LoanName = l.LoanName,
                TotalAmount = l.TotalAmount,
                RemainingAmount = l.RemainingAmount,
                MonthlyDeduction = l.MonthlyDeduction,
                InstallmentMonths = l.InstallmentMonths,
                IsActive = l.IsActive,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Notes = l.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (loan == null)
        {
            return Error.NotFound("Loan.NotFound", "Loan not found");
        }

        return new GenericResponse<MyLoanDetailsDto>
        {
            Success = true,
            Message = "Loan details retrieved successfully",
            Data = loan
        };
    }
}
