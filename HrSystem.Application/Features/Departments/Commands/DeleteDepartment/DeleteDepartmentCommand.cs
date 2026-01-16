using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
