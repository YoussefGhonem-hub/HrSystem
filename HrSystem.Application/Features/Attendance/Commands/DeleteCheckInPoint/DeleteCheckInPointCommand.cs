using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.DeleteCheckInPoint;

public record DeleteCheckInPointCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteCheckInPointCommandHandler
    : IRequestHandler<DeleteCheckInPointCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteCheckInPointCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteCheckInPointCommand request, CancellationToken cancellationToken)
    {
        var point = await _context.BranchCheckInPoints
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (point is null)
            return Error.NotFound("CheckInPoint.NotFound", "Check-in point not found.");

        point.IsDeleted = true;
        point.DeletedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Check-in point deleted successfully."
        };
    }
}
