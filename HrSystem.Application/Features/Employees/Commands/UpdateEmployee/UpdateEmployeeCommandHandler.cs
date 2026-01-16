using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        // Map updates using Mapster
        request.Employee.Adapt(employee);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        var dto = updatedEmployee.Adapt<EmployeeDto>();

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee updated successfully",
            Data = dto
        };
    }
}
