using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Queries.GetReviewStatuses;

/// <summary>
/// Query to get all active review statuses for dropdown
/// </summary>
public record GetReviewStatusesQuery : IRequest<ErrorOr<GenericResponse<List<ReviewStatusDto>>>>;

public class GetReviewStatusesQueryHandler : IRequestHandler<GetReviewStatusesQuery, ErrorOr<GenericResponse<List<ReviewStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetReviewStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<ReviewStatusDto>>>> Handle(
        GetReviewStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.ReviewStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ReviewStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                Description = s.DescriptionEn,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<ReviewStatusDto>>
        {
            Success = true,
            Message = "Review statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class ReviewStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
