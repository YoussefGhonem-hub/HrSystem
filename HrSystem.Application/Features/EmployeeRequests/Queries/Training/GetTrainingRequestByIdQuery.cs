using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Training;

public record GetTrainingRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetTrainingRequestByIdQueryHandler : IRequestHandler<GetTrainingRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetTrainingRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetTrainingRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.TrainingDetail)
                .ThenInclude(t => t!.TrainingType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Training request not found.");

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = employeeRequest.RequestTypeId,
            RequestTypeName = employeeRequest.RequestTypeRef?.Code ?? "",
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = employeeRequest.Employee?.FullNameEn,
            BranchId = employeeRequest.BranchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            ManagerComments = employeeRequest.ManagerComments,
            RejectionReason = employeeRequest.RejectionReason,
            ApprovedBy = employeeRequest.ApprovedBy,
            ApprovedDate = employeeRequest.ApprovedDate,
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            TrainingDetail = employeeRequest.TrainingDetail != null ? new TrainingDetailDto
            {
                TrainingTypeId = employeeRequest.TrainingDetail.TrainingTypeId,
                TrainingTypeName = employeeRequest.TrainingDetail.TrainingType?.NameEn,
                TrainingName = employeeRequest.TrainingDetail.TrainingName,
                TrainingProvider = employeeRequest.TrainingDetail.TrainingProvider,
                TrainingLocation = employeeRequest.TrainingDetail.TrainingLocation,
                TrainingStartDate = employeeRequest.TrainingDetail.TrainingStartDate,
                TrainingEndDate = employeeRequest.TrainingDetail.TrainingEndDate,
                DurationDays = employeeRequest.TrainingDetail.DurationDays,
                EstimatedCost = employeeRequest.TrainingDetail.EstimatedCost,
                ApprovedBudget = employeeRequest.TrainingDetail.ApprovedBudget,
                Currency = employeeRequest.TrainingDetail.Currency,
                Objectives = employeeRequest.TrainingDetail.Objectives,
                ExpectedOutcome = employeeRequest.TrainingDetail.ExpectedOutcome,
                CertificationObtained = employeeRequest.TrainingDetail.CertificationObtained,
                CertificateUrl = employeeRequest.TrainingDetail.CertificateUrl
            } : null
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Training request retrieved successfully");
    }
}
