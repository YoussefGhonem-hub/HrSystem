using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationById;

public class GetKPIEvaluationByIdQueryHandler : IRequestHandler<GetKPIEvaluationByIdQuery, ErrorOr<GenericResponse<KPIEvaluationDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetKPIEvaluationByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIEvaluationDto>>> Handle(
        GetKPIEvaluationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var kpiEvaluation = await _context.KPIEvaluations
            .Include(ke => ke.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(ke => ke.KPI)
            .FirstOrDefaultAsync(ke => ke.Id == request.Id, cancellationToken);

        if (kpiEvaluation == null)
        {
            return Error.NotFound(description: "KPI evaluation not found");
        }

        var dto = new KPIEvaluationDto
        {
            Id = kpiEvaluation.Id,
            PerformanceReviewId = kpiEvaluation.PerformanceReviewId,
            EmployeeName = kpiEvaluation.PerformanceReview.Employee.FullNameEn,
            KPIId = kpiEvaluation.KPIId,
            KPINameEn = kpiEvaluation.KPI.NameEn,
            KPINameAr = kpiEvaluation.KPI.NameAr,
            Rating = kpiEvaluation.Rating,
            WeightedScore = kpiEvaluation.WeightedScore,
            Comments = kpiEvaluation.Comments,
            Evidence = kpiEvaluation.Evidence,
            CreatedDate = kpiEvaluation.CreatedDate.DateTime
        };

        return new GenericResponse<KPIEvaluationDto>
        {
            Success = true,
            Data = dto
        };
    }
}
