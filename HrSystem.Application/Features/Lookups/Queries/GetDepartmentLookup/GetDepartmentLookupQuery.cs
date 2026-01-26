using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetDepartmentLookup;

public record GetDepartmentLookupQuery() : IRequest<ErrorOr<GenericResponse<List<DepartmentLookupDto>>>>;

public class GetDepartmentLookupQueryHandler : IRequestHandler<GetDepartmentLookupQuery, ErrorOr<GenericResponse<List<DepartmentLookupDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetDepartmentLookupQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<DepartmentLookupDto>>>> Handle(GetDepartmentLookupQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .OrderBy(d => d.NameEn)
            .Select(d => new DepartmentLookupDto
            {
                Id = d.Id,
                NameEn = d.NameEn,
                NameAr = d.NameAr
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<DepartmentLookupDto>>
        {
            Success = true,
            Message = "Departments retrieved successfully",
            Data = departments
        };
    }
}
