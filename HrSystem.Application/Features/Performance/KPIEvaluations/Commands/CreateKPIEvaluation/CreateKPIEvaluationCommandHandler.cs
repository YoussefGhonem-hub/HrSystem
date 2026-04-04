using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.CreateKPIEvaluation;

public class CreateKPIEvaluationCommandHandler : IRequestHandler<CreateKPIEvaluationCommand, ErrorOr<GenericResponse<KPIEvaluationDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateKPIEvaluationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIEvaluationDto>>> Handle(
        CreateKPIEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var kpiEvaluation = new KPIEvaluation
        {
            PerformanceReviewId = request.PerformanceReviewId,
            KPIId = request.KPIId,
            Rating = request.Rating,
            WeightedScore = request.WeightedScore,
            Comments = request.Comments,
            Evidence = request.Evidence,
            TenantId = Guid.Empty
        };

        _context.KPIEvaluations.Add(kpiEvaluation);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdKPIEvaluation = await _context.KPIEvaluations
            .Include(ke => ke.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(ke => ke.KPI)
            .FirstAsync(ke => ke.Id == kpiEvaluation.Id, cancellationToken);

        var dto = new KPIEvaluationDto
        {
            Id = createdKPIEvaluation.Id,
            PerformanceReviewId = createdKPIEvaluation.PerformanceReviewId,
            EmployeeName = createdKPIEvaluation.PerformanceReview.Employee.FullNameEn,
            KPIId = createdKPIEvaluation.KPIId,
            KPINameEn = createdKPIEvaluation.KPI.NameEn,
            KPINameAr = createdKPIEvaluation.KPI.NameAr,
            Rating = createdKPIEvaluation.Rating,
            WeightedScore = createdKPIEvaluation.WeightedScore,
            Comments = createdKPIEvaluation.Comments,
            Evidence = createdKPIEvaluation.Evidence,
            CreatedDate = createdKPIEvaluation.CreatedDate.DateTime
        };

        return new GenericResponse<KPIEvaluationDto>
        {
            Success = true,
            Data = dto,
            Message = "KPI evaluation created successfully"
        };
    }
}
