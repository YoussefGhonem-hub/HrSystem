using ErrorOr;
using HrSystem.Application.Features.Employees.DTOs;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
