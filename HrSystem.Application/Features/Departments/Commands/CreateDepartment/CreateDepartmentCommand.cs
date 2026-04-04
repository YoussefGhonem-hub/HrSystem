using ErrorOr;
using HrSystem.Application.Features.Departments.Queries.GetDepartmentById;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(
    string NameAr,
    string NameEn,
    string? Description,
    Guid? ManagerId,
    Guid? ParentDepartmentId,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<DepartmentDto>>>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, ErrorOr<GenericResponse<DepartmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateDepartmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DepartmentDto>>> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = new Department
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            ManagerId = request.ManagerId,
            ParentDepartmentId = request.ParentDepartmentId,
            BranchId = request.BranchId,
            TenantId = Guid.Empty
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        var createdDepartment = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstAsync(d => d.Id == department.Id, cancellationToken);

        var dto = new DepartmentDto
        {
            Id = createdDepartment.Id,
            NameAr = createdDepartment.NameAr,
            NameEn = createdDepartment.NameEn,
            Description = createdDepartment.Description,
            ManagerId = createdDepartment.ManagerId,
            ManagerName = createdDepartment.Manager?.FullNameEn,
            ParentDepartmentId = createdDepartment.ParentDepartmentId,
            ParentDepartmentName = createdDepartment.ParentDepartment?.NameEn,
            BranchId = createdDepartment.BranchId,
            BranchName = createdDepartment.Branch?.NameEn,
            EmployeeCount = createdDepartment.Employees.Count
        };

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Message = "Department created successfully",
            Data = dto
        };
    }
}
