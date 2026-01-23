using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyLoans;

/// <summary>
/// Query to get loan list for the currently logged-in user
/// </summary>
public record GetMyLoansQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<ErrorOr<GenericResponse<PagedResult<MyLoanSummaryDto>>>>;

public class GetMyLoansQueryHandler : IRequestHandler<GetMyLoansQuery, ErrorOr<GenericResponse<PagedResult<MyLoanSummaryDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyLoansQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<MyLoanSummaryDto>>>> Handle(
        GetMyLoansQuery request,
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

        var loansQuery = _context.Loans
            .Where(l => !l.IsDeleted && l.EmployeeId == employeeId.Value)
            .OrderByDescending(l => l.CreatedDate)
            .Select(l => new MyLoanSummaryDto
            {
                LoanId = l.Id,
                LoanName = l.LoanName,
                TotalAmount = l.TotalAmount,
                RemainingAmount = l.RemainingAmount,
                MonthlyDeduction = l.MonthlyDeduction,
                InstallmentMonths = l.InstallmentMonths,
                IsActive = l.IsActive
            });

        var pagedResult = await loansQuery.ToPagedResultAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new GenericResponse<PagedResult<MyLoanSummaryDto>>
        {
            Success = true,
            Message = "Loans retrieved successfully",
            Data = pagedResult
        };
    }
}
