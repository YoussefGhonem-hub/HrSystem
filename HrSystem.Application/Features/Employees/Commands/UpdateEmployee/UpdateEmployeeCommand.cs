using ErrorOr;
using HrSystem.Application.Features.Employees.DTOs;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(Guid Id, UpdateEmployeeDto Employee) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
