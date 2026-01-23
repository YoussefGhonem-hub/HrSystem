using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetEmployeeStatuses;

/// <summary>
/// Query to get all active employee statuses for dropdown
/// </summary>
public record GetEmployeeStatusesQuery : IRequest<ErrorOr<GenericResponse<List<EmployeeStatusDto>>>>;

public class GetEmployeeStatusesQueryHandler : IRequestHandler<GetEmployeeStatusesQuery, ErrorOr<GenericResponse<List<EmployeeStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeStatusDto>>>> Handle(
        GetEmployeeStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.EmployeeStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new EmployeeStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                Description = s.Description,
                ColorCode = s.ColorCode,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<EmployeeStatusDto>>
        {
            Success = true,
            Message = "Employee statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class EmployeeStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
}
