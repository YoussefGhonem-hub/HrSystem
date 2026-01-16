using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.DeleteKPIEvaluation;

public class DeleteKPIEvaluationCommandHandler : IRequestHandler<DeleteKPIEvaluationCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteKPIEvaluationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteKPIEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var kpiEvaluation = await _context.KPIEvaluations
            .FirstOrDefaultAsync(ke => ke.Id == request.Id, cancellationToken);

        if (kpiEvaluation == null)
        {
            return Error.NotFound(description: "KPI evaluation not found");
        }

        _context.KPIEvaluations.Remove(kpiEvaluation);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "KPI evaluation deleted successfully"
        };
    }
}
