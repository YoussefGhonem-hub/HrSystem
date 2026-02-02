using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    string? AttachmentUrl,
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

    // Count approved permissions for monthly limit
    private static readonly EmployeeRequestStatus[] ApprovedStatuses =
    {
        EmployeeRequestStatus.Approved,
        EmployeeRequestStatus.Completed
    };

    private readonly ApplicationDbContext _context;

    public CreatePermissionRequestCommandHandler(ApplicationDbContext context) => _context = context;

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

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        // Check branch settings
        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestType == EmployeeRequestType.Permission,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Permission requests are not allowed for this branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required.");

        // Check max open requests
        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestType == EmployeeRequestType.Permission
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

        // Validate hours per request
        if (permissionType.MaxHoursPerRequest.HasValue && request.TotalHours > permissionType.MaxHoursPerRequest.Value)
        {
            return Error.Validation(description: $"Total hours ({request.TotalHours}) exceeds maximum allowed per request ({permissionType.MaxHoursPerRequest.Value} hours).");
        }

        // Validate monthly hours limit
        if (permissionType.MaxHoursPerMonth.HasValue)
        {
            var startOfMonth = new DateTime(request.PermissionDate.Year, request.PermissionDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Get total approved hours for this month (same permission type)
            var usedHoursThisMonth = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(pd => pd.PermissionTypeId == request.PermissionTypeId
                            && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                            && pd.PermissionDate >= startOfMonth
                            && pd.PermissionDate <= endOfMonth
                            && ApprovedStatuses.Contains(pd.EmployeeRequest.Status))
                .SumAsync(pd => pd.TotalHours, cancellationToken);

            // Also include pending requests to prevent over-booking
            var pendingHoursThisMonth = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(pd => pd.PermissionTypeId == request.PermissionTypeId
                            && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                            && pd.PermissionDate >= startOfMonth
                            && pd.PermissionDate <= endOfMonth
                            && pd.EmployeeRequest.Status == EmployeeRequestStatus.Pending)
                .SumAsync(pd => pd.TotalHours, cancellationToken);

            var totalHoursUsed = usedHoursThisMonth + pendingHoursThisMonth;
            var remainingHours = permissionType.MaxHoursPerMonth.Value - totalHoursUsed;

            if (request.TotalHours > remainingHours)
            {
                return Error.Validation(description: 
                    $"Monthly hours limit exceeded. Used: {totalHoursUsed} hours, Remaining: {remainingHours} hours, Requested: {request.TotalHours} hours. Maximum allowed per month: {permissionType.MaxHoursPerMonth.Value} hours.");
            }
        }

        // Create EmployeeRequest
        var employeeRequest = new EmployeeRequest
        {
            RequestType = EmployeeRequestType.Permission,
            Status = EmployeeRequestStatus.Pending,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.PermissionDate.Date.Add(request.FromTime ?? TimeSpan.Zero),
            EndDate = request.PermissionDate.Date.Add(request.ToTime ?? TimeSpan.FromHours((double)request.TotalHours)),
            AttachmentUrl = request.AttachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        // Calculate leave deduction if applicable
        decimal? leaveDeduction = null;
        if (permissionType.DeductsFromLeave && permissionType.HoursPerLeaveDay.HasValue && permissionType.HoursPerLeaveDay.Value > 0)
        {
            leaveDeduction = request.TotalHours / permissionType.HoursPerLeaveDay.Value;
        }

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
            LeaveDeduction = leaveDeduction,
            TenantId = employee.TenantId,
            BranchId = branchId
        };

        employeeRequest.PermissionDetail = permissionDetail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Build response DTO
        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestType = employeeRequest.RequestType,
            RequestTypeName = employeeRequest.RequestType.ToString(),
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
