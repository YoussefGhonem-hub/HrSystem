using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
        {
            return Error.NotFound(description: "Department not found");
        }

        // Check if department has employees
        if (department.Employees.Any())
        {
            return Error.Conflict(description: "Cannot delete department with assigned employees");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Department deleted successfully"
        };
    }
}
