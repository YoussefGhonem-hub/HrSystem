using ErrorOr;
using HrSystem.Application.Features.Departments.Queries.GetDepartmentById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.AssignDepartmentManager;

public record AssignDepartmentManagerCommand(Guid DepartmentId, Guid EmployeeId)
    : IRequest<ErrorOr<GenericResponse<DepartmentDto>>>;

public class AssignDepartmentManagerCommandHandler : IRequestHandler<AssignDepartmentManagerCommand, ErrorOr<GenericResponse<DepartmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public AssignDepartmentManagerCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<DepartmentDto>>> Handle(
        AssignDepartmentManagerCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);

        if (department is null)
        {
            return Error.NotFound("Department.NotFound", "Department not found");
        }

        var employee = await _context.Employees
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        if (department.BranchId.HasValue && employee.BranchId.HasValue && department.BranchId != employee.BranchId)
        {
            return Error.Validation("Branch.Mismatch", "Employee must belong to the same branch as the department");
        }

        department.ManagerId = employee.Id;
        await _context.SaveChangesAsync(cancellationToken);

        // Load full dto
        var dtoDepartment = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

        if (dtoDepartment is null)
        {
            return Error.Unexpected(description: "Failed to load updated department");
        }

        var dto = new DepartmentDto
        {
            Id = dtoDepartment.Id,
            NameAr = dtoDepartment.NameAr,
            NameEn = dtoDepartment.NameEn,
            Description = dtoDepartment.Description,
            ManagerId = dtoDepartment.ManagerId,
            ManagerName = dtoDepartment.Manager?.FullNameEn ?? dtoDepartment.Manager?.FullNameAr,
            ParentDepartmentId = dtoDepartment.ParentDepartmentId,
            ParentDepartmentName = dtoDepartment.ParentDepartment?.NameEn ?? dtoDepartment.ParentDepartment?.NameAr,
            BranchId = dtoDepartment.BranchId,
            BranchName = dtoDepartment.Branch?.NameEn,
            EmployeeCount = dtoDepartment.Employees?.Count ?? 0,
            CreatedDate = dtoDepartment.CreatedDate.DateTime
        };

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Message = "Department manager assigned successfully",
            Data = dto
        };
    }
}
