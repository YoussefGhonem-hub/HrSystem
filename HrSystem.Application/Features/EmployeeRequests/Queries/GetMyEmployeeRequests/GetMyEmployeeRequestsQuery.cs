using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetMyEmployeeRequests;

public record GetMyEmployeeRequestsQuery(Guid EmployeeId, string? RequestTypeCode = null)
    : IRequest<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>;

public class GetMyEmployeeRequestsQueryHandler
    : IRequestHandler<GetMyEmployeeRequestsQuery, ErrorOr<GenericResponse<List<EmployeeRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyEmployeeRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeRequestDto>>>> Handle(
        GetMyEmployeeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e!.DirectManager)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Where(r => r.EmployeeId == request.EmployeeId);

        if (!string.IsNullOrEmpty(request.RequestTypeCode))
        {
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);
        }

        var items = await query
            .OrderByDescending(r => r.CreatedDate)
            .Take(200)
            .ToListAsync(cancellationToken);

        var correctionTypesById = await _context.AttendanceCorrectionTypes
            .AsNoTracking()
            .Where(t => t.IsActive && !t.IsDeleted)
            .Select(t => new { t.Id, t.NameEn })
            .ToDictionaryAsync(t => t.Id, t => t.NameEn, cancellationToken);

        var dtos = items.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee?.FullNameEn,
            EmployeeCode = r.Employee?.EmployeeCode,
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
            ApprovedByName = r.ApprovedByUser != null
                ? (!string.IsNullOrWhiteSpace(r.ApprovedByUser.FullName)
                    ? r.ApprovedByUser.FullName
                    : r.ApprovedByUser.UserName)
                : null,
            ApprovedDate = r.ApprovedDate,
            ProcessedBy = r.ProcessedBy,
            ProcessedByName = r.ProcessedByUser != null
                ? (!string.IsNullOrWhiteSpace(r.ProcessedByUser.FullName)
                    ? r.ProcessedByUser.FullName
                    : r.ProcessedByUser.UserName)
                : null,
            ProcessedDate = r.ProcessedDate,
            PendingAt = r.Status == EmployeeRequestStatus.Pending
                ? (r.Employee != null && r.Employee.DirectManager != null 
                    ? r.Employee.DirectManager.FullNameEn 
                    : "HR Department")
                : r.Status == EmployeeRequestStatus.ManagerApproved
                    ? "HR Department"
                    : null,
            AttendanceCorrectionDetail = BuildAttendanceCorrectionDetail(r.Description, correctionTypesById),
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
                : null
        }).ToList();

        return GenericResponse<List<EmployeeRequestDto>>.SuccessResult(dtos);
    }

    private static AttendanceCorrectionDetailDto? BuildAttendanceCorrectionDetail(
        string? description,
        IReadOnlyDictionary<Guid, string> correctionTypesById)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "AttendanceCorrectionTypeId", out var correctionTypeIdRaw) ||
            !Guid.TryParse(correctionTypeIdRaw, out var correctionTypeId))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "AttendanceDate", out var attendanceDateRaw) ||
            !DateTime.TryParse(attendanceDateRaw, out var attendanceDate))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "CorrectedTime", out var correctedTimeRaw) ||
            !TimeSpan.TryParse(correctedTimeRaw, out var correctedTime))
        {
            return null;
        }

        correctionTypesById.TryGetValue(correctionTypeId, out var correctionTypeName);

        return new AttendanceCorrectionDetailDto
        {
            AttendanceCorrectionTypeId = correctionTypeId,
            AttendanceCorrectionTypeName = correctionTypeName,
            AttendanceDate = attendanceDate.Date,
            CorrectedTime = correctedTime
        };
    }

    private static bool TryExtractDescriptionValue(string description, string key, out string value)
    {
        value = string.Empty;
        var prefix = key + ":";
        var lines = description.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = line.Substring(prefix.Length).Trim();
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }
}
