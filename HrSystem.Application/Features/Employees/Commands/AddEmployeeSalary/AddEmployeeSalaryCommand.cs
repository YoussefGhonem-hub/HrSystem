using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.AddEmployeeSalary;

public record AddEmployeeSalaryCommand(
    Guid EmployeeId,
    decimal BasicSalary,
    DateTime EffectiveDate,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<EmployeeSalaryDto>>>;
