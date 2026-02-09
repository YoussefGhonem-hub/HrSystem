using System;
using System.Collections.Generic;
using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetEmployeeRequests;

/// <summary>
/// Query to get all requests for a specific employee with optional status filtering.
/// Used by managers/HR to view an employee's requests.
/// </summary>
public record GetEmployeeRequestsQuery(
    Guid EmployeeId,
    string? RequestTypeCode = null,
    IReadOnlyCollection<string>? RequestTypeCodes = null,
    EmployeeRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestsWithStatsDto>>>;

public record EmployeeRequestsWithStatsDto
{
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public List<EmployeeRequestDto> Requests { get; init; } = new();
    public RequestStatsDto Stats { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public record RequestStatsDto
{
    public int Draft { get; init; }
    public int Pending { get; init; }
    public int ManagerApproved { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
    public int Cancelled { get; init; }
    public int Completed { get; init; }
    public int Total { get; init; }
}

public class GetEmployeeRequestsQueryHandler
    : IRequestHandler<GetEmployeeRequestsQuery, ErrorOr<GenericResponse<EmployeeRequestsWithStatsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestsWithStatsDto>>> Handle(
        GetEmployeeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify employee exists
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var singleTypeCodeFilter = string.IsNullOrWhiteSpace(request.RequestTypeCode)
            ? null
            : request.RequestTypeCode.Trim();

        var multipleTypeCodesFilter = request.RequestTypeCodes?
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (multipleTypeCodesFilter is { Length: 0 })
        {
            multipleTypeCodesFilter = null;
        }

        // Build base query
        var baseQuery = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (multipleTypeCodesFilter is { Length: > 0 })
        {
            baseQuery = baseQuery.Where(r => r.RequestTypeRef != null && multipleTypeCodesFilter.Contains(r.RequestTypeRef.Code));
        }
        else if (!string.IsNullOrEmpty(singleTypeCodeFilter))
        {
            baseQuery = baseQuery.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == singleTypeCodeFilter);
        }

        // Get stats (before any filtering)
        var allRequests = await baseQuery.ToListAsync(cancellationToken);
        var stats = new RequestStatsDto
        {
            Draft = allRequests.Count(r => r.Status == EmployeeRequestStatus.Draft),
            Pending = allRequests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApproved = allRequests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            Approved = allRequests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            Rejected = allRequests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            Cancelled = allRequests.Count(r => r.Status == EmployeeRequestStatus.Cancelled),
            Completed = allRequests.Count(r => r.Status == EmployeeRequestStatus.Completed),
            Total = allRequests.Count
        };

        // Apply filters for paginated results
        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (multipleTypeCodesFilter is { Length: > 0 })
            query = query.Where(r => r.RequestTypeRef != null && multipleTypeCodesFilter.Contains(r.RequestTypeRef.Code));
        else if (!string.IsNullOrEmpty(singleTypeCodeFilter))
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == singleTypeCodeFilter);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(r => r.CreatedDate)
            .ThenByDescending(r => r.RequestedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var employeeName = $"{employee.FirstNameEn} {employee.LastNameEn}";
        
        var dtos = entities.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            EmployeeName = employeeName,
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
            OvertimeDetail = r.OvertimeDetail != null
                ? new OvertimeDetailDto
                {
                    OvertimeTypeId = r.OvertimeDetail.OvertimeTypeId,
                    OvertimeTypeName = r.OvertimeDetail.OvertimeType?.NameEn,
                    OvertimeDate = r.OvertimeDetail.OvertimeDate,
                    PlannedHours = r.OvertimeDetail.PlannedHours,
                    ActualHours = r.OvertimeDetail.ActualHours,
                    Multiplier = r.OvertimeDetail.Multiplier,
                    ProjectCode = r.OvertimeDetail.ProjectCode,
                    TaskDescription = r.OvertimeDetail.TaskDescription,
                    ApprovedBy = r.OvertimeDetail.ApprovedBy,
                    ApprovedDate = r.OvertimeDetail.ApprovedDate,
                    ApprovalNotes = r.OvertimeDetail.ApprovalNotes
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
                : null,
            MiscellaneousDetail = r.MiscellaneousDetail != null
                ? new MiscellaneousDetailDto
                {
                    MiscellaneousTypeId = r.MiscellaneousDetail.MiscellaneousTypeId,
                    MiscellaneousTypeName = r.MiscellaneousDetail.MiscellaneousType?.NameEn,
                    AdditionalNotes = r.MiscellaneousDetail.AdditionalNotes,
                    ReferenceNumber = r.MiscellaneousDetail.ReferenceNumber,
                    Priority = r.MiscellaneousDetail.Priority,
                    ExpectedCompletionDate = r.MiscellaneousDetail.ExpectedCompletionDate
                }
                : null,
            PersonalDetail = r.PersonalDetail != null
                ? new PersonalDetailDto
                {
                    PersonalTypeId = r.PersonalDetail.PersonalTypeId,
                    PersonalTypeName = r.PersonalDetail.PersonalType?.NameEn,
                    Reason = r.PersonalDetail.Reason,
                    IsUrgent = r.PersonalDetail.IsUrgent,
                    RequiresConfidentiality = r.PersonalDetail.RequiresConfidentiality
                }
                : null,
            FeedbackDetail = r.FeedbackDetail != null
                ? new FeedbackDetailDto
                {
                    FeedbackTypeId = r.FeedbackDetail.FeedbackTypeId,
                    FeedbackTypeName = r.FeedbackDetail.FeedbackType?.NameEn,
                    FeedbackContent = r.FeedbackDetail.FeedbackContent,
                    IsAnonymous = r.FeedbackDetail.IsAnonymous,
                    Rating = r.FeedbackDetail.Rating,
                    TargetDepartment = r.FeedbackDetail.TargetDepartment,
                    TargetPerson = r.FeedbackDetail.TargetPerson,
                    SuggestedImprovement = r.FeedbackDetail.SuggestedImprovement,
                    ResponseRequired = r.FeedbackDetail.ResponseRequired,
                    ResponseContent = r.FeedbackDetail.ResponseContent,
                    ResponseDate = r.FeedbackDetail.ResponseDate
                }
                : null,
            PermissionDetail = r.PermissionDetail != null
                ? new PermissionDetailDto
                {
                    PermissionTypeId = r.PermissionDetail.PermissionTypeId,
                    PermissionTypeName = r.PermissionDetail.PermissionType?.NameEn,
                    PermissionDate = r.PermissionDetail.PermissionDate,
                    FromTime = r.PermissionDetail.FromTime,
                    ToTime = r.PermissionDetail.ToTime,
                    TotalHours = r.PermissionDetail.TotalHours,
                    Reason = r.PermissionDetail.Reason,
                    ManagerId = r.PermissionDetail.ManagerId,
                    ManagerApprovalDate = r.PermissionDetail.ManagerApprovalDate,
                    ManagerComments = r.PermissionDetail.ManagerComments,
                    LeaveDeduction = r.PermissionDetail.LeaveDeduction
                }
                : null
        }).ToList();

        var result = new EmployeeRequestsWithStatsDto
        {
            EmployeeId = request.EmployeeId,
            EmployeeName = employeeName,
            Requests = dtos,
            Stats = stats,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return GenericResponse<EmployeeRequestsWithStatsDto>.SuccessResult(result, $"Found {totalCount} requests.");
    }
}
