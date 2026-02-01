using System;
using System.Collections.Generic;
using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Shared.Common;
using MediatR;
using EmployeeDto = HrSystem.Application.Features.Employees.Queries.GetEmployeeById.EmployeeDto;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePayroll;

public record UpdateEmployeePayrollCommand(
    Guid EmployeeId,
    decimal BasicSalary,
    DateTime EffectiveDate,
    string? Currency,
    bool IncludeSocialInsurance,
    decimal? SocialInsuranceEmployeeRate,
    decimal? SocialInsuranceEmployerRate,
    string? PaymentMethod,
    PayrollBankInfoPayload? BankInfo,
    List<PayrollAllowancePayload>? Allowances,
    List<PayrollDeductionPayload>? Deductions,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
