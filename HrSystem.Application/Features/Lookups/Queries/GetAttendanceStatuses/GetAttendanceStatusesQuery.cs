using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetAttendanceStatuses;

/// <summary>
/// Query to get all active attendance statuses for dropdown
/// </summary>
public record GetAttendanceStatusesQuery : IRequest<ErrorOr<GenericResponse<List<AttendanceStatusDto>>>>;

public class GetAttendanceStatusesQueryHandler : IRequestHandler<GetAttendanceStatusesQuery, ErrorOr<GenericResponse<List<AttendanceStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendanceStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<AttendanceStatusDto>>>> Handle(
        GetAttendanceStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.AttendanceStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new AttendanceStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                Description = s.Description,
                ColorCode = s.ColorCode,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<AttendanceStatusDto>>
        {
            Success = true,
            Message = "Attendance statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class AttendanceStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
}
