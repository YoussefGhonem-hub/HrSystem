using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<DepartmentDto>>>;
