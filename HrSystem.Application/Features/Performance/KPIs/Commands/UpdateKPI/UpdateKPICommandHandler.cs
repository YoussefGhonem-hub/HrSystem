using ErrorOr;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.UpdateKPI;

public class UpdateKPICommandHandler : IRequestHandler<UpdateKPICommand, ErrorOr<GenericResponse<KPIDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateKPICommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIDto>>> Handle(
        UpdateKPICommand request,
        CancellationToken cancellationToken)
    {
        var kpi = await _context.KPIs
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken);

        if (kpi == null)
        {
            return Error.NotFound(description: "KPI not found");
        }

        kpi.NameAr = request.NameAr;
        kpi.NameEn = request.NameEn;
        kpi.DescriptionAr = request.DescriptionAr;
        kpi.DescriptionEn = request.DescriptionEn;
        kpi.Category = request.Category;
        kpi.Weight = request.Weight;
        kpi.MeasurementCriteria = request.MeasurementCriteria;
        kpi.JobTitleId = request.JobTitleId;
        kpi.DepartmentId = request.DepartmentId;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedKPI = await _context.KPIs
            .Include(k => k.JobTitle)
            .Include(k => k.Department)
            .FirstAsync(k => k.Id == kpi.Id, cancellationToken);

        var dto = new KPIDto
        {
            Id = updatedKPI.Id,
            NameAr = updatedKPI.NameAr,
            NameEn = updatedKPI.NameEn,
            DescriptionAr = updatedKPI.DescriptionAr,
            DescriptionEn = updatedKPI.DescriptionEn,
            Category = updatedKPI.Category,
            Weight = updatedKPI.Weight,
            MeasurementCriteria = updatedKPI.MeasurementCriteria,
            JobTitleId = updatedKPI.JobTitleId,
            JobTitleEn = updatedKPI.JobTitle?.TitleEn,
            JobTitleAr = updatedKPI.JobTitle?.TitleAr,
            DepartmentId = updatedKPI.DepartmentId,
            DepartmentNameEn = updatedKPI.Department?.NameEn,
            DepartmentNameAr = updatedKPI.Department?.NameAr,
            CreatedDate = updatedKPI.CreatedDate.DateTime
        };

        return new GenericResponse<KPIDto>
        {
            Success = true,
            Data = dto,
            Message = "KPI updated successfully"
        };
    }
}
