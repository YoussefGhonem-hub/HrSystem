using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, ErrorOr<GenericResponse<DepartmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DepartmentDto>>> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
        {
            return Error.NotFound(description: "Department not found");
        }

        var dto = department.Adapt<DepartmentDto>();

        return new GenericResponse<DepartmentDto>
        {
            Success = true,
            Data = dto
        };
    }
}
