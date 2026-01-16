using ErrorOr;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.CreateKPI;

public class CreateKPICommandHandler : IRequestHandler<CreateKPICommand, ErrorOr<GenericResponse<KPIDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateKPICommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<KPIDto>>> Handle(
        CreateKPICommand request,
        CancellationToken cancellationToken)
    {
        var kpi = new KPI
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            Category = request.Category,
            Weight = request.Weight,
            MeasurementCriteria = request.MeasurementCriteria,
            JobTitleId = request.JobTitleId,
            DepartmentId = request.DepartmentId,
            TenantId = Guid.NewGuid() // Should come from CurrentUser.OrganizationId
        };

        _context.KPIs.Add(kpi);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdKPI = await _context.KPIs
            .Include(k => k.JobTitle)
            .Include(k => k.Department)
            .FirstAsync(k => k.Id == kpi.Id, cancellationToken);

        var dto = new KPIDto
        {
            Id = createdKPI.Id,
            NameAr = createdKPI.NameAr,
            NameEn = createdKPI.NameEn,
            DescriptionAr = createdKPI.DescriptionAr,
            DescriptionEn = createdKPI.DescriptionEn,
            Category = createdKPI.Category,
            Weight = createdKPI.Weight,
            MeasurementCriteria = createdKPI.MeasurementCriteria,
            JobTitleId = createdKPI.JobTitleId,
            JobTitleEn = createdKPI.JobTitle?.TitleEn,
            JobTitleAr = createdKPI.JobTitle?.TitleAr,
            DepartmentId = createdKPI.DepartmentId,
            DepartmentNameEn = createdKPI.Department?.NameEn,
            DepartmentNameAr = createdKPI.Department?.NameAr,
            CreatedDate = createdKPI.CreatedDate.DateTime
        };

        return new GenericResponse<KPIDto>
        {
            Success = true,
            Data = dto,
            Message = "KPI created successfully"
        };
    }
}
