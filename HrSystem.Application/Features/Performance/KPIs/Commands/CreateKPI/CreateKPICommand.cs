using ErrorOr;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.CreateKPI;

public record CreateKPICommand(
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string Category,
    int Weight,
    string MeasurementCriteria,
    Guid? JobTitleId,
    Guid? DepartmentId
) : IRequest<ErrorOr<GenericResponse<KPIDto>>>;
