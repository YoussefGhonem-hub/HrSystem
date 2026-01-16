using ErrorOr;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIById;

public class GetKPIByIdQueryHandler : IRequestHandler<GetKPIByIdQuery, ErrorOr<GenericResponse<KPIDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetKPIByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIDto>>> Handle(
        GetKPIByIdQuery request,
        CancellationToken cancellationToken)
    {
        var kpi = await _context.KPIs
            .Include(k => k.JobTitle)
            .Include(k => k.Department)
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken);

        if (kpi == null)
        {
            return Error.NotFound(description: "KPI not found");
        }

        var dto = new KPIDto
        {
            Id = kpi.Id,
            NameAr = kpi.NameAr,
            NameEn = kpi.NameEn,
            DescriptionAr = kpi.DescriptionAr,
            DescriptionEn = kpi.DescriptionEn,
            Category = kpi.Category,
            Weight = kpi.Weight,
            MeasurementCriteria = kpi.MeasurementCriteria,
            JobTitleId = kpi.JobTitleId,
            JobTitleEn = kpi.JobTitle?.TitleEn,
            JobTitleAr = kpi.JobTitle?.TitleAr,
            DepartmentId = kpi.DepartmentId,
            DepartmentNameEn = kpi.Department?.NameEn,
            DepartmentNameAr = kpi.Department?.NameAr,
            CreatedDate = kpi.CreatedDate.DateTime
        };

        return new GenericResponse<KPIDto>
        {
            Success = true,
            Data = dto
        };
    }
}
