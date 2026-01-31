using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeJobInfo;

public class UpdateEmployeeJobInfoCommandHandler : IRequestHandler<UpdateEmployeeJobInfoCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeJobInfoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeJobInfoCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        employee.DepartmentId = request.DepartmentId;
        employee.JobTitleId = request.JobTitleId;
        employee.DirectManagerId = request.DirectManagerId;
        employee.BranchId = request.BranchId;
        employee.ContractTypeId = request.ContractTypeId;
        employee.HiringDate = request.HiringDate;
        employee.ProbationPeriodMonths = request.ProbationPeriodMonths;
        employee.ProbationEndDate = request.HiringDate.AddMonths(request.ProbationPeriodMonths);
        employee.StatusId = EmployeeStatusIds.Active;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee job info updated successfully",
            Data = dto
        };
    }
}
