using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Common;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;

public record CreateVacationRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid VacationTypeId,
    decimal TotalDays,
    IFormFile? Attachment,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateVacationRequestCommandHandler
    : IRequestHandler<CreateVacationRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreateVacationRequestCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateVacationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        // Get RequestType by Code
        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Vacation", cancellationToken);
        
        if (requestType == null)
            return Error.NotFound(description: "Vacation request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Vacation requests are not allowed for this branch.");

        if (requestType.RequireAttachment && (request.Attachment == null || request.Attachment.Length == 0))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open vacation requests reached.");
        }

        // Overlap validation — reject full, partial, and contained overlaps.
        // Only active (non-cancelled, non-rejected) requests are checked.
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate.Date;
        var hasOverlap = await _context.EmployeeRequests
            .AnyAsync(r =>
                r.EmployeeId == request.EmployeeId &&
                r.RequestTypeId == requestType.Id &&
                OpenStatuses.Contains(r.Status) &&
                r.StartDate.HasValue && r.EndDate.HasValue &&
                r.StartDate.Value.Date <= endDate &&
                r.EndDate.Value.Date >= startDate,
                cancellationToken);

        if (hasOverlap)
            return Error.Conflict(
                description: "A vacation request for the selected dates (or overlapping dates) already exists. Cancel or modify the existing request first.");

        var vacationType = await _context.VacationTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Id == request.VacationTypeId, cancellationToken);

        if (vacationType == null)
            return Error.Validation(description: "Invalid vacation type.");

        var initialStatus = await EmployeeRequestWorkflowHelper.ResolveInitialStatusAsync(
            _context,
            employee.DirectManagerId,
            vacationType.RequiresManagerApproval,
            cancellationToken);

        // Upload attachment to S3 if provided
        string? attachmentUrl = null;
        if (request.Attachment != null && request.Attachment.Length > 0)
        {
            var stored = await _storageService.Upload(request.Attachment, cancellationToken);
            if (string.IsNullOrWhiteSpace(stored.Key))
                return Error.Failure(description: "File upload failed.");
            attachmentUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);
        }

        // Create EmployeeRequest
        var employeeRequest = new EmployeeRequest
        {
            RequestTypeId = requestType.Id,
            Status = initialStatus,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AttachmentUrl = attachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        // Create VacationDetail payload
        var vacationDetail = new VacationRequestDetail
        {
            VacationTypeId = request.VacationTypeId,
            TotalDays = request.TotalDays,
            ManagerId = employee.DirectManagerId,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone
        };

        employeeRequest.VacationDetail = vacationDetail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = requestType.Id,
            RequestTypeName = requestType.Code,
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            BranchId = employeeRequest.BranchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            VacationDetail = new VacationDetailDto
            {
                VacationTypeId = vacationDetail.VacationTypeId,
                VacationTypeName = vacationType.NameEn,
                TotalDays = vacationDetail.TotalDays,
                ManagerId = vacationDetail.ManagerId,
                EmergencyContactName = vacationDetail.EmergencyContactName,
                EmergencyContactPhone = vacationDetail.EmergencyContactPhone
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Vacation request submitted successfully.");
    }
}
