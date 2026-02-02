using System;
using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeAttendance;

public record UpdateEmployeeAttendanceCommand(
    Guid EmployeeId,
    string? WorkShift,
    string? WorkDays,
    string? GracePeriod,
    string? MaxLatePerMonth,
    bool OvertimeEligible,
    string? AttendanceMethod,
    string? LateDeductionPolicy,
    string? AbsenceDeductionPolicy,
    string? HalfDayRule,
    string? MissingCheckoutHandling
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
