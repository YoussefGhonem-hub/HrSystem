using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetMyRequests;

public record GetMyRequestsQuery(
    Guid EmployeeId,
    string? RequestTypeCode = null,
    EmployeeRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>;

public class GetMyRequestsQueryHandler
    : IRequestHandler<GetMyRequestsQuery, ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>> Handle(
        GetMyRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e!.DirectManager)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (!string.IsNullOrEmpty(request.RequestTypeCode))
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(r => r.RequestedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            BranchId = r.BranchId,
            Title = r.Title,
            Description = r.Description,
            RequestedDate = r.RequestedDate,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            AttachmentUrl = r.AttachmentUrl,
            ManagerComments = r.ManagerComments,
            RejectionReason = r.RejectionReason,
            ApprovedBy = r.ApprovedBy,
            ApprovedDate = r.ApprovedDate,
            PendingAt = r.Status == EmployeeRequestStatus.Pending
                ? (r.Employee != null && r.Employee.DirectManager != null 
                    ? r.Employee.DirectManager.FullNameEn 
                    : "HR Department")
                : r.Status == EmployeeRequestStatus.ManagerApproved
                    ? "HR Department"
                    : null,
            VacationDetail = r.VacationDetail != null
                ? new VacationDetailDto
                {
                    VacationTypeId = r.VacationDetail.VacationTypeId,
                    VacationTypeName = r.VacationDetail.VacationType?.NameEn,
                    TotalDays = r.VacationDetail.TotalDays,
                    ManagerId = r.VacationDetail.ManagerId,
                    ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
                    EmergencyContactName = r.VacationDetail.EmergencyContactName,
                    EmergencyContactPhone = r.VacationDetail.EmergencyContactPhone
                }
                : null,
            TrainingDetail = r.TrainingDetail != null
                ? new TrainingDetailDto
                {
                    TrainingTypeId = r.TrainingDetail.TrainingTypeId,
                    TrainingTypeName = r.TrainingDetail.TrainingType?.NameEn,
                    TrainingName = r.TrainingDetail.TrainingName,
                    TrainingProvider = r.TrainingDetail.TrainingProvider,
                    TrainingLocation = r.TrainingDetail.TrainingLocation,
                    TrainingStartDate = r.TrainingDetail.TrainingStartDate,
                    TrainingEndDate = r.TrainingDetail.TrainingEndDate,
                    DurationDays = r.TrainingDetail.DurationDays,
                    EstimatedCost = r.TrainingDetail.EstimatedCost,
                    ApprovedBudget = r.TrainingDetail.ApprovedBudget,
                    Currency = r.TrainingDetail.Currency,
                    Objectives = r.TrainingDetail.Objectives,
                    ExpectedOutcome = r.TrainingDetail.ExpectedOutcome,
                    CertificationObtained = r.TrainingDetail.CertificationObtained,
                    CertificateUrl = r.TrainingDetail.CertificateUrl
                }
                : null
        }).ToList();

        return GenericResponse<List<EmployeeRequestDto>>.SuccessResult(dtos, $"Found {totalCount} requests.");
    }
}
