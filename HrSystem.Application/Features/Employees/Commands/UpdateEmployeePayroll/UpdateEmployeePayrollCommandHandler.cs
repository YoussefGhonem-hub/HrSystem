using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using EmployeeDto = HrSystem.Application.Features.Employees.Queries.GetEmployeeById.EmployeeDto;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePayroll;

public class UpdateEmployeePayrollCommandHandler : IRequestHandler<UpdateEmployeePayrollCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _context;

    public UpdateEmployeePayrollCommandHandler(ISender sender, ApplicationDbContext context)
    {
        _sender = sender;
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(UpdateEmployeePayrollCommand request, CancellationToken cancellationToken)
    {
        var configureCommand = new ConfigureEmployeePayrollCommand(
            request.EmployeeId,
            request.BasicSalary,
            request.EffectiveDate,
            request.Currency,
            request.IncludeSocialInsurance,
            request.SocialInsuranceEmployeeRate,
            request.SocialInsuranceEmployerRate,
            request.PaymentMethod,
            request.BankInfo,
            request.Allowances,
            request.Deductions,
            request.Notes
        );

        var configureResult = await _sender.Send(configureCommand, cancellationToken);
        if (configureResult.IsError)
        {
            return configureResult.Errors;
        }

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, request.EmployeeId, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee payroll updated successfully",
            Data = dto
        };
    }
}
