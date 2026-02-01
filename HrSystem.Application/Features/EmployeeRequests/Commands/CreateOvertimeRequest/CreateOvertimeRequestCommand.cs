using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;

public record CreateOvertimeRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime OvertimeDate,
    TimeSpan PlannedHours,
    decimal Multiplier,
    string? ProjectCode,
    string? TaskDescription,
    string? AttachmentUrl,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateOvertimeRequestCommandHandler
    : IRequestHandler<CreateOvertimeRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;

    public CreateOvertimeRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateOvertimeRequestCommand request,
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
                s => s.BranchId == branchId && s.RequestType == EmployeeRequestType.OverTime,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Overtime requests are not allowed for this branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestType == EmployeeRequestType.OverTime
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open overtime requests reached.");
        }

        var employeeRequest = new EmployeeRequest
        {
            RequestType = EmployeeRequestType.OverTime,
            Status = EmployeeRequestStatus.Pending,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.OvertimeDate,
            AttachmentUrl = request.AttachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        var overtimeDetail = new OvertimeRequestDetail
        {
            OvertimeDate = request.OvertimeDate,
            PlannedHours = request.PlannedHours,
            Multiplier = request.Multiplier,
            ProjectCode = request.ProjectCode,
            TaskDescription = request.TaskDescription,
            TenantId = employee.TenantId,
            BranchId = branchId
        };

        employeeRequest.OvertimeDetail = overtimeDetail;

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
            AttachmentUrl = employeeRequest.AttachmentUrl,
            OvertimeDetail = new OvertimeDetailDto
            {
                OvertimeDate = overtimeDetail.OvertimeDate,
                PlannedHours = overtimeDetail.PlannedHours,
                Multiplier = overtimeDetail.Multiplier,
                ProjectCode = overtimeDetail.ProjectCode,
                TaskDescription = overtimeDetail.TaskDescription
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Overtime request submitted successfully.");
    }
}
