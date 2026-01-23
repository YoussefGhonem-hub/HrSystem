using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.AddEmployeeSalary;

public class AddEmployeeSalaryCommandHandler : IRequestHandler<AddEmployeeSalaryCommand, ErrorOr<GenericResponse<EmployeeSalaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public AddEmployeeSalaryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeSalaryDto>>> Handle(
        AddEmployeeSalaryCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("Salary.Unauthorized", "Not allowed to add employee salary");
            }
        }

        var currentSalary = await _context.Salaries
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.EmployeeId == request.EmployeeId && s.IsCurrent, cancellationToken);

        if (currentSalary != null)
        {
            currentSalary.IsCurrent = false;
            currentSalary.EndDate = request.EffectiveDate > currentSalary.EffectiveDate
                ? request.EffectiveDate.AddDays(-1)
                : request.EffectiveDate;
            currentSalary.MarkAsModified(CurrentUser.Id ?? Guid.Empty);
        }

        var salary = new Salary
        {
            EmployeeId = request.EmployeeId,
            BasicSalary = request.BasicSalary,
            EffectiveDate = request.EffectiveDate,
            Notes = request.Notes,
            IsCurrent = true,
            TenantId = employee.TenantId != Guid.Empty
                ? employee.TenantId
                : CurrentUser.OrganizationId ?? Guid.Empty
        };

        salary.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeSalaryDto
        {
            Id = salary.Id,
            EmployeeId = salary.EmployeeId,
            BasicSalary = salary.BasicSalary,
            EffectiveDate = salary.EffectiveDate,
            EndDate = salary.EndDate,
            Notes = salary.Notes,
            IsCurrent = salary.IsCurrent
        };

        return new GenericResponse<EmployeeSalaryDto>
        {
            Success = true,
            Message = "Employee salary added successfully",
            Data = dto
        };
    }
}
