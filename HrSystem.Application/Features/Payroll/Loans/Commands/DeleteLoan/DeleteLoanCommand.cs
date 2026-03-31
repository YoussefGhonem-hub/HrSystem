using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Commands.DeleteLoan;

public record DeleteLoanCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteLoanCommandHandler : IRequestHandler<DeleteLoanCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteLoanCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteLoanCommand request,
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
            return Error.Forbidden("Loan.BranchMismatch", "You can only delete loans for employees in your branch");
        }

        loan.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Loan deleted successfully"
        };
    }
}
