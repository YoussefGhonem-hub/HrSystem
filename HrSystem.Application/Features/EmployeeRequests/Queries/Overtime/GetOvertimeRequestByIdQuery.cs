using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;

public record GetOvertimeRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetOvertimeRequestByIdQueryHandler
    : IRequestHandler<GetOvertimeRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeRequestByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetOvertimeRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.OvertimeDetail)
                .ThenInclude(o => o!.OvertimeType)
            .FirstOrDefaultAsync(r => r.Id == request.Id
                && r.RequestTypeRef != null
                && r.RequestTypeRef.Code == "OverTime", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Overtime request not found.");

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = employeeRequest.RequestTypeId,
            RequestTypeName = employeeRequest.RequestTypeRef?.Code ?? "",
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = employeeRequest.Employee?.FullNameEn,
            BranchId = employeeRequest.BranchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            ManagerComments = employeeRequest.ManagerComments,
            RejectionReason = employeeRequest.RejectionReason,
            ApprovedBy = employeeRequest.ApprovedBy,
            ApprovedDate = employeeRequest.ApprovedDate,
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            OvertimeDetail = employeeRequest.OvertimeDetail != null ? new OvertimeDetailDto
            {
                OvertimeTypeId = employeeRequest.OvertimeDetail.OvertimeTypeId,
                OvertimeTypeName = employeeRequest.OvertimeDetail.OvertimeType?.NameEn,
                OvertimeDate = employeeRequest.OvertimeDetail.OvertimeDate,
                PlannedHours = employeeRequest.OvertimeDetail.PlannedHours,
                ActualHours = employeeRequest.OvertimeDetail.ActualHours,
                Multiplier = employeeRequest.OvertimeDetail.Multiplier,
                ProjectCode = employeeRequest.OvertimeDetail.ProjectCode,
                TaskDescription = employeeRequest.OvertimeDetail.TaskDescription,
                ApprovedBy = employeeRequest.OvertimeDetail.ApprovedBy,
                ApprovedDate = employeeRequest.OvertimeDetail.ApprovedDate,
                ApprovalNotes = employeeRequest.OvertimeDetail.ApprovalNotes
            } : null
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Overtime request retrieved successfully");
    }
}
