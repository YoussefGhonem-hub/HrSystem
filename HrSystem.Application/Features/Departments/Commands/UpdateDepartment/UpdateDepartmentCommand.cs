using ErrorOr;
using HrSystem.Application.Features.Departments.Queries.GetDepartmentById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentDto Department) : IRequest<ErrorOr<GenericResponse<DepartmentDto>>>;
