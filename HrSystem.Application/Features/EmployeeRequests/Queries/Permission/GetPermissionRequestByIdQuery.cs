using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

public record GetPermissionRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetPermissionRequestByIdQueryHandler : IRequestHandler<GetPermissionRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(GetPermissionRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
            .Include(r => r.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Branch)
            .Include(r => r.PermissionDetail)
                .ThenInclude(p => p!.PermissionType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission", cancellationToken);

        if (entity == null)
            return Error.NotFound("Permission.NotFound", "Permission request not found");

        var dto = new EmployeeRequestDto
        {
            Id = entity.Id,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeName = entity.RequestTypeRef?.Code ?? "",
            Status = entity.Status,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee?.FullNameEn,
            BranchId = entity.BranchId,
            Title = entity.Title,
            Description = entity.Description,
            RequestedDate = entity.RequestedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            AttachmentUrl = entity.AttachmentUrl,
            ManagerComments = entity.ManagerComments,
            RejectionReason = entity.RejectionReason,
            ApprovedBy = entity.ApprovedBy,
            ApprovedDate = entity.ApprovedDate,
            ProcessedBy = entity.ProcessedBy,
            ProcessedDate = entity.ProcessedDate,
            PermissionDetail = entity.PermissionDetail != null
                ? new PermissionDetailDto
                {
                    PermissionTypeId = entity.PermissionDetail.PermissionTypeId,
                    PermissionTypeName = entity.PermissionDetail.PermissionType?.NameEn,
                    PermissionDate = entity.PermissionDetail.PermissionDate,
                    FromTime = entity.PermissionDetail.FromTime,
                    ToTime = entity.PermissionDetail.ToTime,
                    TotalHours = entity.PermissionDetail.TotalHours,
                    Reason = entity.PermissionDetail.Reason,
                    ManagerId = entity.PermissionDetail.ManagerId,
                    ManagerApprovalDate = entity.PermissionDetail.ManagerApprovalDate,
                    ManagerComments = entity.PermissionDetail.ManagerComments,
                    LeaveDeduction = entity.PermissionDetail.LeaveDeduction
                }
                : null
        };

        return new GenericResponse<EmployeeRequestDto>
        {
            Success = true,
            Message = "Permission request retrieved successfully",
            Data = dto
        };
    }
}
