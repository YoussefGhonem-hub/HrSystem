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
            EndDate = loan.EndDate,
            IsActive = loan.IsActive,
            Notes = loan.Notes
        };

        return new GenericResponse<LoanDto>
        {
            Success = true,
            Message = "Loan retrieved successfully",
            Data = dto
        };
    }
}
