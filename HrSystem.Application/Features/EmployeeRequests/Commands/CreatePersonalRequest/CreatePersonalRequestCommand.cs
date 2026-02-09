using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreatePersonalRequest;

public record CreatePersonalRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid PersonalTypeId,
    string Reason,
    bool IsUrgent,
    bool RequiresConfidentiality,
    string? PreferredContactMethod,
    string? AdditionalContactInfo,
    string? AttachmentUrl,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreatePersonalRequestCommandHandler
    : IRequestHandler<CreatePersonalRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;

    public CreatePersonalRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreatePersonalRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Personal", cancellationToken);

        if (requestType == null)
            return Error.NotFound(description: "Personal request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Personal requests are not allowed for this branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open personal requests reached.");
        }

        var personalType = await _context.PersonalTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.PersonalTypeId && t.IsActive, cancellationToken);

        if (personalType == null)
            return Error.Validation(description: "Invalid personal type.");

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

        var detail = new PersonalRequestDetail
        {
            PersonalTypeId = request.PersonalTypeId,
            Reason = request.Reason.Trim(),
            IsUrgent = request.IsUrgent,
            RequiresConfidentiality = request.RequiresConfidentiality,
            PreferredContactMethod = request.PreferredContactMethod,
            AdditionalContactInfo = request.AdditionalContactInfo
        };

        employeeRequest.PersonalDetail = detail;

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
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            PersonalDetail = new PersonalDetailDto
            {
                PersonalTypeId = detail.PersonalTypeId,
                PersonalTypeName = personalType.NameEn,
                Reason = detail.Reason,
                IsUrgent = detail.IsUrgent,
                RequiresConfidentiality = detail.RequiresConfidentiality
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Personal request submitted successfully.");
    }
}
