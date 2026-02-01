using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetMyEmployeeRequests;

public record GetMyEmployeeRequestsQuery(Guid EmployeeId, EmployeeRequestType? RequestType = null)
    : IRequest<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>;

public class GetMyEmployeeRequestsQueryHandler
    : IRequestHandler<GetMyEmployeeRequestsQuery, ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyEmployeeRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>> Handle(
        GetMyEmployeeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (request.RequestType.HasValue)
        {
            query = query.Where(r => r.RequestType == request.RequestType.Value);
        }

        var items = await query
            .OrderByDescending(r => r.CreatedDate)
            .Take(200)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestType = r.RequestType,
            RequestTypeName = r.RequestType.ToString(),
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            BranchId = r.BranchId,
            Title = r.Title,
            Description = r.Description,
            RequestedDate = r.RequestedDate,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            AttachmentUrl = r.AttachmentUrl,
            ManagerComments = r.ManagerComments,
            RejectionReason = r.RejectionReason,
            ApprovedBy = r.ApprovedBy,
            ApprovedDate = r.ApprovedDate
        }).ToList();

        return GenericResponse<List<EmployeeRequestDto>>.SuccessResult(dtos);
    }
}
