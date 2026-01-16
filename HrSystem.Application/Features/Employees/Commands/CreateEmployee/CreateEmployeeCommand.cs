using ErrorOr;
using HrSystem.Application.Features.Employees.DTOs;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(CreateEmployeeDto Employee) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
