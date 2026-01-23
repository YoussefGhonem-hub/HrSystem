using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetLeaveStatuses;

/// <summary>
/// Query to get all active leave statuses for dropdown
/// </summary>
public record GetLeaveStatusesQuery : IRequest<ErrorOr<GenericResponse<List<LeaveStatusDto>>>>;

public class GetLeaveStatusesQueryHandler : IRequestHandler<GetLeaveStatusesQuery, ErrorOr<GenericResponse<List<LeaveStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeaveStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<LeaveStatusDto>>>> Handle(
        GetLeaveStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.LeaveStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new LeaveStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<LeaveStatusDto>>
        {
            Success = true,
            Message = "Leave statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class LeaveStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
