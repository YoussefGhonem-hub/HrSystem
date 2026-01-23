using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Queries.GetReviewTypes;

/// <summary>
/// Query to get all active review types for dropdown
/// </summary>
public record GetReviewTypesQuery : IRequest<ErrorOr<GenericResponse<List<ReviewTypeDto>>>>;

public class GetReviewTypesQueryHandler : IRequestHandler<GetReviewTypesQuery, ErrorOr<GenericResponse<List<ReviewTypeDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetReviewTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<ReviewTypeDto>>>> Handle(
        GetReviewTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await _context.ReviewTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new ReviewTypeDto
            {
                Id = t.Id,
                NameEn = t.NameEn,
                NameAr = t.NameAr,
                Description = t.DescriptionEn,
                DisplayOrder = t.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<ReviewTypeDto>>
        {
            Success = true,
            Message = "Review types retrieved successfully",
            Data = types
        };
    }
}

public class ReviewTypeDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
