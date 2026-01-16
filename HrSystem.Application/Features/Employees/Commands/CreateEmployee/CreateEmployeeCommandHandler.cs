using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = request.Employee.Adapt<Employee>();
        employee.TenantId = Guid.NewGuid(); // Should come from CurrentUser.OrganizationId

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        var dto = createdEmployee.Adapt<EmployeeDto>();

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee created successfully",
            Data = dto
        };
    }
}
