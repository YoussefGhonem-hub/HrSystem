using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;

public record CreateVacationRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid VacationTypeId,
    decimal TotalDays,
    string? AttachmentUrl,
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
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;

    public CreateVacationRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateVacationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestType == EmployeeRequestType.Vacation,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Vacation requests are not allowed for this branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestType == EmployeeRequestType.Vacation
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open vacation requests reached.");
        }

        var vacationType = await _context.VacationTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Id == request.VacationTypeId, cancellationToken);

        if (vacationType == null)
            return Error.Validation(description: "Invalid vacation type.");

        // Find matching LeaveType by name (maps VacationType to LeaveType for balance tracking)
        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.NameEn == vacationType.NameEn || lt.NameAr == vacationType.NameAr, cancellationToken);

        // Find LeavePolicy for this leave type
        LeavePolicy? leavePolicy = null;

        if (leaveType != null)
        {
            leavePolicy = await _context.LeavePolicies
                .FirstOrDefaultAsync(lp => lp.LeaveTypeId == leaveType.Id, cancellationToken);

            if (leavePolicy != null)
            {
                // Check leave balance
                var currentYear = DateTime.UtcNow.Year;
                var leaveBalance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                                               && lb.LeavePolicyId == leavePolicy.Id
                                               && lb.Year == currentYear, cancellationToken);

                if (leaveBalance != null && leaveBalance.RemainingDays < request.TotalDays)
                {
                    return Error.Validation(description: $"Insufficient leave balance. Available: {leaveBalance.RemainingDays} days, Requested: {request.TotalDays} days.");
                }
            }
        }

        // Create EmployeeRequest
        var employeeRequest = new EmployeeRequest
        {
            RequestType = EmployeeRequestType.Vacation,
            Status = EmployeeRequestStatus.Pending,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AttachmentUrl = request.AttachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        // Create VacationDetail with LeaveType/LeavePolicy links for balance tracking
        var vacationDetail = new VacationRequestDetail
        {
            VacationTypeId = request.VacationTypeId,
            TotalDays = request.TotalDays,
            LeaveTypeId = leaveType?.Id,
            LeavePolicyId = leavePolicy?.Id,
            ManagerId = employee.DirectManagerId,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            TenantId = employee.TenantId,
            BranchId = branchId
        };

        employeeRequest.VacationDetail = vacationDetail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestType = employeeRequest.RequestType,
            RequestTypeName = employeeRequest.RequestType.ToString(),
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
