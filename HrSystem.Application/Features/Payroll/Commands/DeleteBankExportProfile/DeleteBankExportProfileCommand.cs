using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.DeleteBankExportProfile;

public record DeleteBankExportProfileCommand(Guid Id) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class DeleteBankExportProfileCommandHandler
    : IRequestHandler<DeleteBankExportProfileCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public DeleteBankExportProfileCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(
        DeleteBankExportProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.BankExportProfiles
            .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, cancellationToken);

        if (profile == null)
            return Error.NotFound(description: "Bank export profile not found.");

        profile.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<bool>.SuccessResult(true, "Bank export profile deleted successfully.");
    }
}
