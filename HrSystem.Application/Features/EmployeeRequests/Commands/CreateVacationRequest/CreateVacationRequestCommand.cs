using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
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

        // Get RequestType by Code
        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Vacation" && rt.TenantId == employee.TenantId, cancellationToken);
        
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

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
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

        var vacationType = await _context.VacationTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Id == request.VacationTypeId, cancellationToken);

        if (vacationType == null)
            return Error.Validation(description: "Invalid vacation type.");

        // Create EmployeeRequest
        var employeeRequest = new EmployeeRequest
        {
            RequestTypeId = requestType.Id,
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

        // Create VacationDetail payload
        var vacationDetail = new VacationRequestDetail
        {
            VacationTypeId = request.VacationTypeId,
            TotalDays = request.TotalDays,
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
