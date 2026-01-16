using ErrorOr;
using HrSystem.Application.Features.Departments.Queries.GetDepartmentById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, ErrorOr<GenericResponse<DepartmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DepartmentDto>>> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
        {
            return Error.NotFound(description: "Department not found");
        }

        request.Department.Adapt(department);

        await _context.SaveChangesAsync(cancellationToken);

        var updatedDepartment = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstAsync(d => d.Id == department.Id, cancellationToken);

        var dto = updatedDepartment.Adapt<DepartmentDto>();

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Message = "Department updated successfully",
            Data = dto
        };
    }
}
