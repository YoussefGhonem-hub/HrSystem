using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;

public record GetVacationRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetVacationRequestByIdQueryHandler : IRequestHandler<GetVacationRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetVacationRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(GetVacationRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
            .Include(r => r.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Branch)
            .Include(r => r.VacationDetail)
                .ThenInclude(v => v!.VacationType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation", cancellationToken);

        if (entity == null)
            return Error.NotFound("Vacation.NotFound", "Vacation request not found");

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
            VacationDetail = entity.VacationDetail != null
                ? new VacationDetailDto
                {
                    VacationTypeId = entity.VacationDetail.VacationTypeId,
                    VacationTypeName = entity.VacationDetail.VacationType?.NameEn,
                    TotalDays = entity.VacationDetail.TotalDays,
                    LeaveTypeId = entity.VacationDetail.LeaveTypeId,
                    LeavePolicyId = entity.VacationDetail.LeavePolicyId,
                    ManagerId = entity.VacationDetail.ManagerId,
                    ManagerApprovalDate = entity.VacationDetail.ManagerApprovalDate,
                    ManagerComments = entity.VacationDetail.ManagerComments,
                    HRApprovedBy = entity.VacationDetail.HRApprovedBy,
                    HRApprovalDate = entity.VacationDetail.HRApprovalDate,
                    HRComments = entity.VacationDetail.HRComments,
                    EmergencyContactName = entity.VacationDetail.EmergencyContactName,
                    EmergencyContactPhone = entity.VacationDetail.EmergencyContactPhone
                }
                : null
        };

        return new GenericResponse<EmployeeRequestDto>
        {
            Success = true,
            Message = "Vacation request retrieved successfully",
            Data = dto
        };
    }
}
