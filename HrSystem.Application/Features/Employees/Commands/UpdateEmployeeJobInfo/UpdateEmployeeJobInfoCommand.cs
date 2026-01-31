using System;
using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeJobInfo;

public record UpdateEmployeeJobInfoCommand(
    Guid EmployeeId,
    Guid DepartmentId,
    Guid JobTitleId,
    Guid? DirectManagerId,
    Guid? BranchId,
    Guid ContractTypeId,
    DateTime HiringDate,
    int ProbationPeriodMonths
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
