using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateEmployeeRequest;

#region Command Models
public record CreateVacationDetailModel(
    Guid VacationTypeId,
    decimal TotalDays,
    Guid? ManagerId,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);

public record CreateTrainingDetailModel(
    Guid TrainingTypeId,
    string TrainingName,
    string? TrainingProvider,
    string? TrainingLocation,
    DateTime TrainingStartDate,
    DateTime TrainingEndDate,
    decimal? EstimatedCost,
    string? Objectives,
    string? ExpectedOutcome
);

public record CreateMiscellaneousDetailModel(
    Guid MiscellaneousTypeId,
    string? AdditionalNotes,
    string? ReferenceNumber,
    string? Priority,
    DateTime? ExpectedCompletionDate
);

public record CreatePersonalDetailModel(
    Guid PersonalTypeId,
    string Reason,
    bool IsUrgent,
    bool RequiresConfidentiality,
    string? PreferredContactMethod,
    string? AdditionalContactInfo
);

public record CreateFeedbackDetailModel(
    Guid FeedbackTypeId,
    string FeedbackContent,
    bool IsAnonymous,
    int? Rating,
    string? TargetDepartment,
    string? TargetPerson,
    string? SuggestedImprovement,
    bool ResponseRequired
);
#endregion

public record CreateEmployeeRequestCommand(
    string RequestTypeCode,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    string? AttachmentUrl,
    Guid EmployeeId,
    Guid? BranchId,
    // Type-specific details (only one should be provided based on RequestType)
    CreateVacationDetailModel? VacationDetail,
    CreateTrainingDetailModel? TrainingDetail,
    CreateMiscellaneousDetailModel? MiscellaneousDetail,
    CreatePersonalDetailModel? PersonalDetail,
    CreateFeedbackDetailModel? FeedbackDetail
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateEmployeeRequestCommandHandler : IRequestHandler<CreateEmployeeRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;

    public CreateEmployeeRequestCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateEmployeeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee record was not found.");

        // Get RequestType by Code
        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == request.RequestTypeCode, cancellationToken);
        
        if (requestType == null)
            return Error.NotFound(description: $"Request type '{request.RequestTypeCode}' not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required for request submission.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.RequestTypeId == requestType.Id, cancellationToken);

        if (branchSetting == null || !branchSetting.IsVisibleToEmployees)
            return Error.Forbidden(description: "This request type is disabled for the selected branch.");

        if (!branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Employees cannot submit this request type for the selected branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required for this request type.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openRequestsCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openRequestsCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "The maximum number of open requests for this type has been reached.");
        }

        // Validate type-specific detail based on RequestType code
        var validationError = await ValidateTypeSpecificDetail(request, cancellationToken);
        if (validationError is not null)
            return validationError.Value;

        var entity = new EmployeeRequest
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

        await _context.EmployeeRequests.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Create type-specific detail
        await CreateTypeSpecificDetail(entity.Id, request, cancellationToken);

        var dto = MapToDto(entity, request, requestType);
        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Request submitted successfully");
    }

    private async Task<Error?> ValidateTypeSpecificDetail(CreateEmployeeRequestCommand request, CancellationToken ct)
    {
        return request.RequestTypeCode switch
        {
            "Vacation" => await ValidateVacationDetail(request.VacationDetail, ct),
            "Training" => await ValidateTrainingDetail(request.TrainingDetail, ct),
            "Miscellaneous" => await ValidateMiscellaneousDetail(request.MiscellaneousDetail, ct),
            "Personal" => await ValidatePersonalDetail(request.PersonalDetail, ct),
            "Feedback" => await ValidateFeedbackDetail(request.FeedbackDetail, ct),
            _ => Error.Validation(description: "Invalid request type.")
        };
    }

    private async Task<Error?> ValidateVacationDetail(CreateVacationDetailModel? detail, CancellationToken ct)
    {
        if (detail is null)
            return Error.Validation(description: "Vacation detail is required for vacation requests.");

        var vacationType = await _context.VacationTypes.AnyAsync(t => t.Id == detail.VacationTypeId && t.IsActive, ct);
        if (!vacationType)
            return Error.Validation(description: "Invalid vacation type selected.");

        return null;
    }

    private async Task<Error?> ValidateTrainingDetail(CreateTrainingDetailModel? detail, CancellationToken ct)
    {
        if (detail is null)
            return Error.Validation(description: "Training detail is required for training requests.");

        var trainingType = await _context.TrainingTypes.AnyAsync(t => t.Id == detail.TrainingTypeId && t.IsActive, ct);
        if (!trainingType)
            return Error.Validation(description: "Invalid training type selected.");

        return null;
    }

    private async Task<Error?> ValidateMiscellaneousDetail(CreateMiscellaneousDetailModel? detail, CancellationToken ct)
    {
        if (detail is null)
            return Error.Validation(description: "Miscellaneous detail is required for miscellaneous requests.");

        var miscType = await _context.MiscellaneousTypes.AnyAsync(t => t.Id == detail.MiscellaneousTypeId && t.IsActive, ct);
        if (!miscType)
            return Error.Validation(description: "Invalid miscellaneous type selected.");

        return null;
    }

    private async Task<Error?> ValidatePersonalDetail(CreatePersonalDetailModel? detail, CancellationToken ct)
    {
        if (detail is null)
            return Error.Validation(description: "Personal detail is required for personal requests.");

        var personalType = await _context.PersonalTypes.AnyAsync(t => t.Id == detail.PersonalTypeId && t.IsActive, ct);
        if (!personalType)
            return Error.Validation(description: "Invalid personal type selected.");

        return null;
    }

    private async Task<Error?> ValidateFeedbackDetail(CreateFeedbackDetailModel? detail, CancellationToken ct)
    {
        if (detail is null)
            return Error.Validation(description: "Feedback detail is required for feedback requests.");

        var feedbackType = await _context.FeedbackTypes.AnyAsync(t => t.Id == detail.FeedbackTypeId && t.IsActive, ct);
        if (!feedbackType)
            return Error.Validation(description: "Invalid feedback type selected.");

        return null;
    }

    private async Task CreateTypeSpecificDetail(Guid requestId, CreateEmployeeRequestCommand request, CancellationToken ct)
    {
        switch (request.RequestTypeCode)
        {
            case "Vacation" when request.VacationDetail is not null:
                await _context.VacationRequestDetails.AddAsync(new VacationRequestDetail
                {
                    EmployeeRequestId = requestId,
                    VacationTypeId = request.VacationDetail.VacationTypeId,
                    TotalDays = request.VacationDetail.TotalDays,
                    ManagerId = request.VacationDetail.ManagerId,
                    EmergencyContactName = request.VacationDetail.EmergencyContactName,
                    EmergencyContactPhone = request.VacationDetail.EmergencyContactPhone
                }, ct);
                break;

            case "Training" when request.TrainingDetail is not null:
                await _context.TrainingRequestDetails.AddAsync(new TrainingRequestDetail
                {
                    EmployeeRequestId = requestId,
                    TrainingTypeId = request.TrainingDetail.TrainingTypeId,
                    TrainingName = request.TrainingDetail.TrainingName,
                    TrainingProvider = request.TrainingDetail.TrainingProvider,
                    TrainingLocation = request.TrainingDetail.TrainingLocation,
                    TrainingStartDate = request.TrainingDetail.TrainingStartDate,
                    TrainingEndDate = request.TrainingDetail.TrainingEndDate,
                    DurationDays = (request.TrainingDetail.TrainingEndDate - request.TrainingDetail.TrainingStartDate).Days + 1,
                    EstimatedCost = request.TrainingDetail.EstimatedCost,
                    Objectives = request.TrainingDetail.Objectives,
                    ExpectedOutcome = request.TrainingDetail.ExpectedOutcome
                }, ct);
                break;

            case "Miscellaneous" when request.MiscellaneousDetail is not null:
                await _context.MiscellaneousRequestDetails.AddAsync(new MiscellaneousRequestDetail
                {
                    EmployeeRequestId = requestId,
                    MiscellaneousTypeId = request.MiscellaneousDetail.MiscellaneousTypeId,
                    AdditionalNotes = request.MiscellaneousDetail.AdditionalNotes,
                    ReferenceNumber = request.MiscellaneousDetail.ReferenceNumber,
                    Priority = request.MiscellaneousDetail.Priority,
                    ExpectedCompletionDate = request.MiscellaneousDetail.ExpectedCompletionDate
                }, ct);
                break;

            case "Personal" when request.PersonalDetail is not null:
                await _context.PersonalRequestDetails.AddAsync(new PersonalRequestDetail
                {
                    EmployeeRequestId = requestId,
                    PersonalTypeId = request.PersonalDetail.PersonalTypeId,
                    Reason = request.PersonalDetail.Reason,
                    IsUrgent = request.PersonalDetail.IsUrgent,
                    RequiresConfidentiality = request.PersonalDetail.RequiresConfidentiality,
                    PreferredContactMethod = request.PersonalDetail.PreferredContactMethod,
                    AdditionalContactInfo = request.PersonalDetail.AdditionalContactInfo
                }, ct);
                break;

            case "Feedback" when request.FeedbackDetail is not null:
                await _context.FeedbackRequestDetails.AddAsync(new FeedbackRequestDetail
                {
                    EmployeeRequestId = requestId,
                    FeedbackTypeId = request.FeedbackDetail.FeedbackTypeId,
                    FeedbackContent = request.FeedbackDetail.FeedbackContent,
                    IsAnonymous = request.FeedbackDetail.IsAnonymous,
                    Rating = request.FeedbackDetail.Rating,
                    TargetDepartment = request.FeedbackDetail.TargetDepartment,
                    TargetPerson = request.FeedbackDetail.TargetPerson,
                    SuggestedImprovement = request.FeedbackDetail.SuggestedImprovement,
                    ResponseRequired = request.FeedbackDetail.ResponseRequired
                }, ct);
                break;
        }

        await _context.SaveChangesAsync(ct);
    }

    private static EmployeeRequestDto MapToDto(EmployeeRequest entity, CreateEmployeeRequestCommand request, Domain.Entities.Requests.RequestType requestType)
    {
        return new EmployeeRequestDto
        {
            Id = entity.Id,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeName = requestType.Code,
            Status = entity.Status,
            EmployeeId = entity.EmployeeId,
            BranchId = entity.BranchId,
            Title = entity.Title,
            Description = entity.Description,
            RequestedDate = entity.RequestedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            AttachmentUrl = entity.AttachmentUrl,
            VacationDetail = request.VacationDetail is not null ? new VacationDetailDto
            {
                VacationTypeId = request.VacationDetail.VacationTypeId,
                TotalDays = request.VacationDetail.TotalDays,
                ManagerId = request.VacationDetail.ManagerId,
                EmergencyContactName = request.VacationDetail.EmergencyContactName,
                EmergencyContactPhone = request.VacationDetail.EmergencyContactPhone
            } : null,
            TrainingDetail = request.TrainingDetail is not null ? new TrainingDetailDto
            {
                TrainingTypeId = request.TrainingDetail.TrainingTypeId,
                TrainingName = request.TrainingDetail.TrainingName,
                TrainingProvider = request.TrainingDetail.TrainingProvider,
                TrainingLocation = request.TrainingDetail.TrainingLocation,
                TrainingStartDate = request.TrainingDetail.TrainingStartDate,
                TrainingEndDate = request.TrainingDetail.TrainingEndDate
            } : null,
            MiscellaneousDetail = request.MiscellaneousDetail is not null ? new MiscellaneousDetailDto
            {
                MiscellaneousTypeId = request.MiscellaneousDetail.MiscellaneousTypeId,
                AdditionalNotes = request.MiscellaneousDetail.AdditionalNotes,
                ReferenceNumber = request.MiscellaneousDetail.ReferenceNumber
            } : null,
            PersonalDetail = request.PersonalDetail is not null ? new PersonalDetailDto
            {
                PersonalTypeId = request.PersonalDetail.PersonalTypeId,
                Reason = request.PersonalDetail.Reason,
                IsUrgent = request.PersonalDetail.IsUrgent
            } : null,
            FeedbackDetail = request.FeedbackDetail is not null ? new FeedbackDetailDto
            {
                FeedbackTypeId = request.FeedbackDetail.FeedbackTypeId,
                FeedbackContent = request.FeedbackDetail.FeedbackContent,
                IsAnonymous = request.FeedbackDetail.IsAnonymous,
                Rating = request.FeedbackDetail.Rating
            } : null
        };
    }
}
