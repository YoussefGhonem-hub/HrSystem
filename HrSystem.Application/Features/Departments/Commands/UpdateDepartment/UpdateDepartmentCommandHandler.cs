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

        department.NameAr = request.NameAr;
        department.NameEn = request.NameEn;
        department.Description = request.Description;
        department.ManagerId = request.ManagerId;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.BranchId = request.BranchId;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedDepartment = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstAsync(d => d.Id == department.Id, cancellationToken);

        var dto = new DepartmentDto
        {
            Id = updatedDepartment.Id,
            NameAr = updatedDepartment.NameAr,
            NameEn = updatedDepartment.NameEn,
            Description = updatedDepartment.Description,
            ManagerId = updatedDepartment.ManagerId,
            ManagerName = updatedDepartment.Manager?.FullNameEn,
            ParentDepartmentId = updatedDepartment.ParentDepartmentId,
            ParentDepartmentName = updatedDepartment.ParentDepartment?.NameEn,
            BranchId = updatedDepartment.BranchId,
            BranchName = updatedDepartment.Branch?.NameEn,
            EmployeeCount = updatedDepartment.Employees.Count
        };

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Message = "Department updated successfully",
            Data = dto
        };
    }
}
