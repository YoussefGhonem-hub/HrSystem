using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;
using System.Collections.Generic;

namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public record ConfigureEmployeePayrollCommand(
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
    string? Notes,
    decimal? OvertimeMultiplier = null
) : IRequest<ErrorOr<GenericResponse<EmployeePayrollConfigurationDto>>>;
