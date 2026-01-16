using ErrorOr;
using HrSystem.Application.Features.Departments.DTOs;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto Department) : IRequest<ErrorOr<GenericResponse<DepartmentDto>>>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, ErrorOr<GenericResponse<DepartmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateDepartmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DepartmentDto>>> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = request.Department.Adapt<Department>();
        department.TenantId = Guid.NewGuid(); // Should come from CurrentUser.OrganizationId

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        var createdDepartment = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstAsync(d => d.Id == department.Id, cancellationToken);

        var dto = createdDepartment.Adapt<DepartmentDto>();

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Message = "Department created successfully",
            Data = dto
        };
    }
}
