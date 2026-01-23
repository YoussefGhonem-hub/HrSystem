using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
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
            .FirstOrDefaultAsync(l => !l.IsDeleted && l.Id == request.Id, cancellationToken);

        if (loan == null)
        {
            return Error.NotFound("Loan.NotFound", "Loan not found");
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
