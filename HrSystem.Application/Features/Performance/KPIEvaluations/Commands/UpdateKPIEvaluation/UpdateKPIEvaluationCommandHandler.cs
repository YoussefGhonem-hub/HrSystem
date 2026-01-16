using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.UpdateKPIEvaluation;

public class UpdateKPIEvaluationCommandHandler : IRequestHandler<UpdateKPIEvaluationCommand, ErrorOr<GenericResponse<KPIEvaluationDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateKPIEvaluationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIEvaluationDto>>> Handle(
        UpdateKPIEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var kpiEvaluation = await _context.KPIEvaluations
            .FirstOrDefaultAsync(ke => ke.Id == request.Id, cancellationToken);

        if (kpiEvaluation == null)
        {
            return Error.NotFound(description: "KPI evaluation not found");
        }

        kpiEvaluation.Rating = request.Rating;
        kpiEvaluation.WeightedScore = request.WeightedScore;
        kpiEvaluation.Comments = request.Comments;
        kpiEvaluation.Evidence = request.Evidence;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedKPIEvaluation = await _context.KPIEvaluations
            .Include(ke => ke.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(ke => ke.KPI)
            .FirstAsync(ke => ke.Id == kpiEvaluation.Id, cancellationToken);

        var dto = new KPIEvaluationDto
        {
            Id = updatedKPIEvaluation.Id,
            PerformanceReviewId = updatedKPIEvaluation.PerformanceReviewId,
            EmployeeName = updatedKPIEvaluation.PerformanceReview.Employee.FullNameEn,
            KPIId = updatedKPIEvaluation.KPIId,
            KPINameEn = updatedKPIEvaluation.KPI.NameEn,
            KPINameAr = updatedKPIEvaluation.KPI.NameAr,
            Rating = updatedKPIEvaluation.Rating,
            WeightedScore = updatedKPIEvaluation.WeightedScore,
            Comments = updatedKPIEvaluation.Comments,
            Evidence = updatedKPIEvaluation.Evidence,
            CreatedDate = updatedKPIEvaluation.CreatedDate.DateTime
        };

        return new GenericResponse<KPIEvaluationDto>
        {
            Success = true,
            Data = dto,
            Message = "KPI evaluation updated successfully"
        };
    }
}
