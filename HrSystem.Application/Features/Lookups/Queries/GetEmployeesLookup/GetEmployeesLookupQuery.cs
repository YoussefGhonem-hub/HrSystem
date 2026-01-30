using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetEmployeesLookup;

public record GetEmployeesLookupQuery() : IRequest<ErrorOr<GenericResponse<List<EmployeeLookupDto>>>>;

public class GetEmployeesLookupQueryHandler : IRequestHandler<GetEmployeesLookupQuery, ErrorOr<GenericResponse<List<EmployeeLookupDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeesLookupQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeLookupDto>>>> Handle(GetEmployeesLookupQuery request, CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .WhereNotDeleted()
            .OrderBy(e => e.FirstNameEn)
            .ThenBy(e => e.LastNameEn)
            .Select(e => new EmployeeLookupDto
            {
                Id = e.Id,
                FullNameEn = e.FirstNameEn + " " + e.LastNameEn,
                FullNameAr = e.FirstNameAr + " " + e.LastNameAr,
                EmployeeCode = e.EmployeeCode
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<EmployeeLookupDto>>
        {
            Success = true,
            Message = "Employees retrieved successfully",
            Data = employees
        };
    }
}
