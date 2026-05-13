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

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateTrainingRequest;

public record CreateTrainingRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    Guid TrainingTypeId,
    string TrainingName,
    string? TrainingProvider,
    string? TrainingLocation,
    DateTime TrainingStartDate,
    DateTime TrainingEndDate,
    int DurationDays,
    decimal? EstimatedCost,
    string? Currency,
    string? Objectives,
    string? ExpectedOutcome,
    IFormFile? Attachment,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateTrainingRequestCommandHandler
    : IRequestHandler<CreateTrainingRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreateTrainingRequestCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateTrainingRequestCommand request,
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
            .FirstOrDefaultAsync(rt => rt.Code == "Training", cancellationToken);
        
        if (requestType == null)
            return Error.NotFound(description: "Training request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Training requests are not allowed for this branch.");

        if (requestType.RequireAttachment && (request.Attachment == null || request.Attachment.Length == 0))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open training requests reached.");
        }

        var trainingType = await _context.TrainingTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TrainingTypeId && t.IsActive, cancellationToken);

        if (trainingType == null)
            return Error.Validation(description: "Invalid training type.");

        var initialStatus = await EmployeeRequestWorkflowHelper.ResolveInitialStatusAsync(
            _context,
            employee.DirectManagerId,
            trainingType.RequiresManagerApproval,
            cancellationToken);

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
            StartDate = request.TrainingStartDate,
            EndDate = request.TrainingEndDate,
            AttachmentUrl = attachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        var trainingDetail = new TrainingRequestDetail
        {
            TrainingTypeId = request.TrainingTypeId,
            TrainingName = request.TrainingName.Trim(),
            TrainingProvider = request.TrainingProvider?.Trim(),
            TrainingLocation = request.TrainingLocation?.Trim(),
            TrainingStartDate = request.TrainingStartDate,
            TrainingEndDate = request.TrainingEndDate,
            DurationDays = request.DurationDays,
            EstimatedCost = request.EstimatedCost,
            Currency = request.Currency ?? "EGP",
            Objectives = request.Objectives,
            ExpectedOutcome = request.ExpectedOutcome
        };

        employeeRequest.TrainingDetail = trainingDetail;

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
            TrainingDetail = new TrainingDetailDto
            {
                TrainingTypeId = trainingDetail.TrainingTypeId,
                TrainingName = trainingDetail.TrainingName,
                TrainingProvider = trainingDetail.TrainingProvider,
                TrainingLocation = trainingDetail.TrainingLocation,
                TrainingStartDate = trainingDetail.TrainingStartDate,
                TrainingEndDate = trainingDetail.TrainingEndDate,
                DurationDays = trainingDetail.DurationDays,
                EstimatedCost = trainingDetail.EstimatedCost,
                Currency = trainingDetail.Currency,
                Objectives = trainingDetail.Objectives,
                ExpectedOutcome = trainingDetail.ExpectedOutcome
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Training request submitted successfully.");
    }
}
