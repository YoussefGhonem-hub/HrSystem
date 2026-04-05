using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreatePermissionRequest;

/// <summary>
/// Command to create a permission request (leave early, come late, short absence).
/// This is for hours-based requests within a day, NOT full-day vacations.
/// Validates against monthly hours limit.
/// </summary>
public record CreatePermissionRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime PermissionDate,
    TimeSpan? FromTime,
    TimeSpan? ToTime,
    decimal TotalHours,
    Guid PermissionTypeId,
    string Reason,
    IFormFile? Attachment,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreatePermissionRequestCommandHandler
    : IRequestHandler<CreatePermissionRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreatePermissionRequestCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreatePermissionRequestCommand request,
        CancellationToken cancellationToken)
    {
        // Validate employee exists
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        // Get RequestType by Code
        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Permission", cancellationToken);
        
        if (requestType == null)
            return Error.NotFound(description: "Permission request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        // Check branch settings
        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Permission requests are not allowed for this branch.");

        if (requestType.RequireAttachment && (request.Attachment == null || request.Attachment.Length == 0))
            return Error.Validation(description: "An attachment is required.");

        // Check max open requests
        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open permission requests reached.");
        }

        // Validate permission type
        var permissionType = await _context.PermissionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.Id == request.PermissionTypeId && pt.IsActive, cancellationToken);

        if (permissionType == null)
            return Error.Validation(description: "Invalid permission type.");

        // Prepare date range for current month
        var currentMonth = request.PermissionDate.Month;
        var currentYear = request.PermissionDate.Year;
        var startOfMonth = new DateTime(currentYear, currentMonth, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // FIRST: Check global permission hours limit (all types combined)
        var globalLimit = await _context.EmployeeGlobalPermissionLimits
            .AsNoTracking()
            .Where(gl => gl.EmployeeId == request.EmployeeId)
            .Select(gl => gl.TotalMonthlyHours)
            .FirstOrDefaultAsync(cancellationToken);

        if (globalLimit.HasValue)
        {
            // Calculate total hours used across ALL permission types this month
            var totalUsedHoursAllTypes = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(pd => pd.EmployeeRequest.EmployeeId == request.EmployeeId
                            && pd.PermissionDate >= startOfMonth
                            && pd.PermissionDate <= endOfMonth
                            && (pd.EmployeeRequest.Status == EmployeeRequestStatus.Pending
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.ManagerApproved
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.Approved
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.Completed))
                .SumAsync(pd => pd.TotalHours, cancellationToken);

            var remainingGlobalHours = globalLimit.Value - totalUsedHoursAllTypes;

            if (request.TotalHours > remainingGlobalHours)
            {
                return Error.Validation(
                    description: $"Permission request exceeds total monthly limit. You have {remainingGlobalHours:F2} hours remaining for ALL permission types this month (Total limit: {globalLimit:F2} hours, Used: {totalUsedHoursAllTypes:F2} hours)."
                );
            }
        }

        // SECOND: Validate per-type monthly hours limit (if global check passed)
        // Get employee-specific limit or fall back to permission type default
        var employeeLimit = await _context.EmployeePermissionLimits
            .AsNoTracking()
            .Where(epl => epl.EmployeeId == request.EmployeeId 
                       && epl.PermissionTypeId == request.PermissionTypeId)
            .Select(epl => epl.MaxHoursPerMonth)
            .FirstOrDefaultAsync(cancellationToken);

        var maxHoursPerMonth = employeeLimit ?? permissionType.DefaultMonthlyHours;

        if (maxHoursPerMonth.HasValue)
        {
            var currentMonthHours = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(pd => pd.PermissionTypeId == request.PermissionTypeId
                            && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                            && pd.PermissionDate >= startOfMonth
                            && pd.PermissionDate <= endOfMonth
                            && (pd.EmployeeRequest.Status == EmployeeRequestStatus.Pending
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.ManagerApproved
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.Approved
                                || pd.EmployeeRequest.Status == EmployeeRequestStatus.Completed))
                .SumAsync(pd => pd.TotalHours, cancellationToken);

            var remainingHours = maxHoursPerMonth.Value - currentMonthHours;

            if (request.TotalHours > remainingHours)
            {
                return Error.Validation(
                    description: $"Permission request exceeds monthly limit. You have {remainingHours:F2} hours remaining for this month (Limit: {maxHoursPerMonth:F2} hours)."
                );
            }
        }

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
            Status = EmployeeRequestStatus.Pending,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.PermissionDate.Date.Add(request.FromTime ?? TimeSpan.Zero),
            EndDate = request.PermissionDate.Date.Add(request.ToTime ?? TimeSpan.FromHours((double)request.TotalHours)),
            AttachmentUrl = attachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        // Create PermissionDetail
        var permissionDetail = new PermissionRequestDetail
        {
            PermissionTypeId = request.PermissionTypeId,
            PermissionDate = request.PermissionDate.Date,
            FromTime = request.FromTime,
            ToTime = request.ToTime,
            TotalHours = request.TotalHours,
            Reason = request.Reason.Trim(),
            ManagerId = employee.DirectManagerId,
            LeaveDeduction = null
        };

        employeeRequest.PermissionDetail = permissionDetail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Build response DTO
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
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            PermissionDetail = new PermissionDetailDto
            {
                PermissionTypeId = permissionDetail.PermissionTypeId,
                PermissionTypeName = permissionType.NameEn,
                PermissionDate = permissionDetail.PermissionDate,
                FromTime = permissionDetail.FromTime,
                ToTime = permissionDetail.ToTime,
                TotalHours = permissionDetail.TotalHours,
                Reason = permissionDetail.Reason,
                ManagerId = permissionDetail.ManagerId,
                LeaveDeduction = permissionDetail.LeaveDeduction
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Permission request created successfully.");
    }
}
