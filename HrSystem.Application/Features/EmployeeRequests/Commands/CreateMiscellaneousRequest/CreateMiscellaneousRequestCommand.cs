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

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateMiscellaneousRequest;

public record CreateMiscellaneousRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid MiscellaneousTypeId,
    string? AdditionalNotes,
    string? ReferenceNumber,
    string? Priority,
    DateTime? ExpectedCompletionDate,
    IFormFile? Attachment,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateMiscellaneousRequestCommandHandler
    : IRequestHandler<CreateMiscellaneousRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreateMiscellaneousRequestCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateMiscellaneousRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Miscellaneous", cancellationToken);

        if (requestType == null)
            return Error.NotFound(description: "Miscellaneous request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Miscellaneous requests are not allowed for this branch.");

        if (requestType.RequireAttachment && (request.Attachment == null || request.Attachment.Length == 0))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open miscellaneous requests reached.");
        }

        var miscType = await _context.MiscellaneousTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.MiscellaneousTypeId && t.IsActive, cancellationToken);

        if (miscType == null)
            return Error.Validation(description: "Invalid miscellaneous type.");

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
            Status = EmployeeRequestStatus.Pending,
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

        var detail = new MiscellaneousRequestDetail
        {
            MiscellaneousTypeId = request.MiscellaneousTypeId,
            AdditionalNotes = request.AdditionalNotes,
            ReferenceNumber = request.ReferenceNumber,
            Priority = request.Priority,
            ExpectedCompletionDate = request.ExpectedCompletionDate
        };

        employeeRequest.MiscellaneousDetail = detail;

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
            MiscellaneousDetail = new MiscellaneousDetailDto
            {
                MiscellaneousTypeId = detail.MiscellaneousTypeId,
                MiscellaneousTypeName = miscType.NameEn,
                AdditionalNotes = detail.AdditionalNotes,
                ReferenceNumber = detail.ReferenceNumber,
                Priority = detail.Priority,
                ExpectedCompletionDate = detail.ExpectedCompletionDate
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Miscellaneous request submitted successfully.");
    }
}
