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

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
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
            AttachmentUrl = request.AttachmentUrl,
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
