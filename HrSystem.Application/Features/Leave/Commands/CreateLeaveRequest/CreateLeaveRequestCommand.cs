using ErrorOr;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequestById;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.CreateLeaveRequest;

public record CreateLeaveRequestCommand(
    Guid? EmployeeId,
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    string? DocumentPath = null,
    string? DocumentUrl = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null
) : IRequest<ErrorOr<GenericResponse<LeaveRequestDetailsDto>>>;

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, ErrorOr<GenericResponse<LeaveRequestDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateLeaveRequestCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeaveRequestDetailsDto>>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // Resolve employee
        var targetEmployeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!targetEmployeeId.HasValue)
        {
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == targetEmployeeId.Value, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        // Validate dates
        if (request.StartDate.Date > request.EndDate.Date)
        {
            return Error.Validation("LeaveRequest.InvalidDates", "Start date must be before or equal to end date");
        }

        // Resolve policy by leave type within tenant scope
        var policy = await _context.LeavePolicies
            .FirstOrDefaultAsync(lp => lp.LeaveTypeId == request.LeaveTypeId, cancellationToken);

        if (policy == null)
        {
            return Error.Validation("LeavePolicy.NotFound", "No leave policy configured for selected type");
        }

        // Compute total days (inclusive)
        var totalDays = (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1m;
        if (totalDays <= 0m)
        {
            return Error.Validation("LeaveRequest.InvalidDuration", "Leave duration must be at least one day");
        }

        // Optional: check remaining balance (if tracked)
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employee.Id && lb.LeavePolicyId == policy.Id && lb.Year == request.StartDate.Year, cancellationToken);

        if (balance != null && balance.RemainingDays < totalDays)
        {
            return Error.Validation("LeaveRequest.InsufficientBalance", "Insufficient leave balance for requested period");
        }

        // Create entity
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeavePolicyId = policy.Id,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TotalDays = totalDays,
            Reason = request.Reason.Trim(),
            LeaveStatusId = LeaveStatusIds.Pending,
            DocumentPath = request.DocumentPath,
            DocumentUrl = request.DocumentUrl,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await _context.LeaveRequests.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Project to details DTO
        var status = await _context.LeaveStatuses.FirstOrDefaultAsync(s => s.Id == entity.LeaveStatusId, cancellationToken);
        var type = await _context.LeaveTypes.FirstOrDefaultAsync(t => t.Id == entity.LeaveTypeId, cancellationToken);

        var dto = new LeaveRequestDetailsDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullNameEn,
            EmployeeNameAr = employee.FullNameAr,
            DepartmentName = employee.Department.NameEn,
            JobTitle = employee.JobTitle.TitleEn,
            BranchName = employee.Branch != null ? employee.Branch.NameEn : null,
            LeaveTypeId = entity.LeaveTypeId,
            LeaveTypeName = type?.NameEn ?? string.Empty,
            LeaveTypeNameAr = type?.NameAr ?? string.Empty,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            TotalDays = entity.TotalDays,
            Reason = entity.Reason,
            StatusId = entity.LeaveStatusId,
            StatusName = status?.NameEn ?? string.Empty,
            StatusNameAr = status?.NameAr ?? string.Empty,
            ManagerApprovalDate = entity.ManagerApprovalDate,
            ManagerComments = entity.ManagerComments,
            HRApprovalDate = entity.HRApprovalDate,
            HRComments = entity.HRComments,
            DocumentUrl = entity.DocumentUrl,
            DocumentPath = entity.DocumentPath,
            DocumentFileName = LeaveRequestDetailsDto.GetFileName(entity.DocumentUrl, entity.DocumentPath),
            CreatedDate = entity.CreatedDate,
            RequiresHRApproval = policy.RequiresHRApproval,
            CurrentApprovalLevel = "Manager",
            ManagerId = entity.ManagerId,
            ManagerName = null,
            ManagerNameAr = null,
            HRApprovedBy = entity.HRApprovedBy,
            HRApprovedByName = null,
            HRApprovedByNameAr = null,
            EmergencyContactName = entity.EmergencyContactName,
            EmergencyContactPhone = entity.EmergencyContactPhone,
            RemainingBalance = balance?.RemainingDays
        };

        dto.ApprovedByName = dto.HRApprovedByName ?? dto.ManagerName;
        dto.ApprovedByNameAr = dto.HRApprovedByNameAr ?? dto.ManagerNameAr;
        dto.ApprovedDate = dto.HRApprovalDate ?? dto.ManagerApprovalDate;

        return new GenericResponse<LeaveRequestDetailsDto>
        {
            Success = true,
            Message = "Leave request created successfully",
            Data = dto
        };
    }
}
