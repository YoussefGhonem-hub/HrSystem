using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.DeleteKPI;

public class DeleteKPICommandHandler : IRequestHandler<DeleteKPICommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteKPICommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteKPICommand request,
        CancellationToken cancellationToken)
    {
        var kpi = await _context.KPIs
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken);

        if (kpi == null)
        {
            return Error.NotFound(description: "KPI not found");
        }

        // Check if KPI is being used in any evaluations
        var hasEvaluations = await _context.KPIEvaluations
            .AnyAsync(ke => ke.KPIId == request.Id, cancellationToken);

        if (hasEvaluations)
        {
            return Error.Conflict(description: "Cannot delete KPI that has evaluations");
        }

        _context.KPIs.Remove(kpi);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "KPI deleted successfully"
        };
    }
}
