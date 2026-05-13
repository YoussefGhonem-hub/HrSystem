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

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;

public record CreateOvertimeRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    Guid OvertimeTypeId,
    DateTime OvertimeDate,
    TimeSpan PlannedHours,
    string? ProjectCode,
    string? TaskDescription,
    IFormFile? Attachment,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateOvertimeRequestCommandHandler
    : IRequestHandler<CreateOvertimeRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreateOvertimeRequestCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "OverTime", cancellationToken);

        if (requestType == null)
            return Error.NotFound(description: "Overtime request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Overtime requests are not allowed for this branch.");

        if (requestType.RequireAttachment && (request.Attachment == null || request.Attachment.Length == 0))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open overtime requests reached.");
        }

        var overtimeType = await _context.OvertimeTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.OvertimeTypeId && t.IsActive, cancellationToken);

        if (overtimeType == null)
            return Error.Validation(description: "Invalid overtime type.");

        var initialStatus = await EmployeeRequestWorkflowHelper.ResolveInitialStatusAsync(
            _context,
            employee.DirectManagerId,
            overtimeType.RequiresManagerApproval,
            cancellationToken);

        // Get employee's salary to use their configured overtime multiplier
        var currentSalary = await _context.Salaries
            .AsNoTracking()
            .Where(s => s.EmployeeId == request.EmployeeId && s.IsCurrent && !s.IsDeleted)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        var multiplier = currentSalary?.OvertimeMultiplier ?? overtimeType.DefaultMultiplier;

        // Upload attachment to S3 if provided
        string? attachmentUrl = null;
        if (request.Attachment != null && request.Attachment.Length > 0)
        {
            var stored = await _storageService.Upload(request.Attachment, cancellationToken);
            if (string.IsNullOrWhiteSpace(stored.Key))
                return Error.Failure(description: "File upload failed.");
            attachmentUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);
        }

        var employeeRequest = new EmployeeRequest
        {
            RequestTypeId = requestType.Id,
            Status = initialStatus,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.OvertimeDate,
            AttachmentUrl = attachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        var detail = new OvertimeRequestDetail
        {
            OvertimeTypeId = request.OvertimeTypeId,
            OvertimeDate = request.OvertimeDate,
            PlannedHours = request.PlannedHours,
            Multiplier = multiplier,
            ProjectCode = request.ProjectCode,
            TaskDescription = request.TaskDescription
        };

        employeeRequest.OvertimeDetail = detail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = requestType.Id,
            RequestTypeName = requestType.Code,
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = $"{employee.FirstNameEn} {employee.LastNameEn}",
            BranchId = branchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            OvertimeDetail = new OvertimeDetailDto
            {
                OvertimeTypeId = detail.OvertimeTypeId,
                OvertimeTypeName = overtimeType.NameEn,
                OvertimeDate = detail.OvertimeDate,
                PlannedHours = detail.PlannedHours,
                Multiplier = detail.Multiplier,
                ProjectCode = detail.ProjectCode,
                TaskDescription = detail.TaskDescription
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Overtime request submitted successfully.");
    }
}
