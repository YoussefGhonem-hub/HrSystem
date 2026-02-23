using ErrorOr;
using HrSystem.Application.Features.Attendance.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetBranchCheckInPoints;

public record GetBranchCheckInPointsQuery(Guid BranchId)
    : IRequest<ErrorOr<GenericResponse<List<CheckInPointDto>>>>;

public class GetBranchCheckInPointsQueryHandler
    : IRequestHandler<GetBranchCheckInPointsQuery, ErrorOr<GenericResponse<List<CheckInPointDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchCheckInPointsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<CheckInPointDto>>>> Handle(
        GetBranchCheckInPointsQuery request, CancellationToken cancellationToken)
    {
        var branchExists = await _context.Branches
            .AnyAsync(b => b.Id == request.BranchId && !b.IsDeleted, cancellationToken);

        if (!branchExists)
            return Error.NotFound("Branch.NotFound", "Branch not found.");

        var points = await _context.BranchCheckInPoints
            .AsNoTracking()
            .Where(p => p.BranchId == request.BranchId && !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new CheckInPointDto
            {
                Id = p.Id,
                BranchId = p.BranchId,
                NameAr = p.NameAr,
                NameEn = p.NameEn,
                Description = p.Description,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                RadiusMeters = p.RadiusMeters,
                IsCheckInPoint = p.IsCheckInPoint,
                IsCheckOutPoint = p.IsCheckOutPoint,
                IsActive = p.IsActive,
                Address = p.Address,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<CheckInPointDto>>
        {
            Success = true,
            Data = points
        };
    }
}
