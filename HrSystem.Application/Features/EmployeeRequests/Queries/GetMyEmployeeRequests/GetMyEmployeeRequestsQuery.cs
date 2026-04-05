using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetMyEmployeeRequests;

public record GetMyEmployeeRequestsQuery(Guid EmployeeId, string? RequestTypeCode = null)
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
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e!.DirectManager)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (!string.IsNullOrEmpty(request.RequestTypeCode))
        {
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);
        }

        var items = await query
            .OrderByDescending(r => r.CreatedDate)
            .Take(200)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
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
            ApprovedDate = r.ApprovedDate,
            PendingAt = r.Status == EmployeeRequestStatus.Pending
                ? (r.Employee != null && r.Employee.DirectManager != null 
                    ? r.Employee.DirectManager.FullNameEn 
                    : "HR Department")
                : r.Status == EmployeeRequestStatus.ManagerApproved
                    ? "HR Department"
                    : null,
            OvertimeDetail = r.OvertimeDetail != null
                ? new OvertimeDetailDto
                {
                    OvertimeTypeId = r.OvertimeDetail.OvertimeTypeId,
                    OvertimeTypeName = r.OvertimeDetail.OvertimeType?.NameEn,
                    OvertimeDate = r.OvertimeDetail.OvertimeDate,
                    PlannedHours = r.OvertimeDetail.PlannedHours,
                    ActualHours = r.OvertimeDetail.ActualHours,
                    Multiplier = r.OvertimeDetail.Multiplier,
                    ProjectCode = r.OvertimeDetail.ProjectCode,
                    TaskDescription = r.OvertimeDetail.TaskDescription,
                    ApprovedBy = r.OvertimeDetail.ApprovedBy,
                    ApprovedDate = r.OvertimeDetail.ApprovedDate,
                    ApprovalNotes = r.OvertimeDetail.ApprovalNotes
                }
                : null
        }).ToList();

        return GenericResponse<List<EmployeeRequestDto>>.SuccessResult(dtos);
    }
}
