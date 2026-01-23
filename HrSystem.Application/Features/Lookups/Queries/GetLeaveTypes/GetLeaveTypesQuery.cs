using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetLeaveTypes;

/// <summary>
/// Query to get all active leave types for dropdown
/// </summary>
public record GetLeaveTypesQuery : IRequest<ErrorOr<GenericResponse<List<LeaveTypeDto>>>>;

public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, ErrorOr<GenericResponse<List<LeaveTypeDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeaveTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<LeaveTypeDto>>>> Handle(
        GetLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        var leaveTypes = await _context.LeaveTypes
            .Where(lt => lt.IsActive)
            .OrderBy(lt => lt.DisplayOrder)
            .Select(lt => new LeaveTypeDto
            {
                Id = lt.Id,
                NameEn = lt.NameEn,
                NameAr = lt.NameAr,
                Description = lt.Description,
                Icon = lt.Icon,
                ColorCode = lt.ColorCode,
                DisplayOrder = lt.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<LeaveTypeDto>>
        {
            Success = true,
            Message = "Leave types retrieved successfully",
            Data = leaveTypes
        };
    }
}

public class LeaveTypeDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
}
