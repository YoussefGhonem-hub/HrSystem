using System.Text.Json;
using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.AuditLogs.Queries.GetAuditLogsList;

public class AuditFieldChangeDto
{
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class AuditLogListDto
{
    public Guid Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? EntityDisplay { get; set; }
    public string Action { get; set; } = string.Empty;
    public int ChangedFieldsCount { get; set; }
    public List<AuditFieldChangeDto> Changes { get; set; } = new();
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public Guid? BranchId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Paged, filterable audit trail for employees, attendance and organization settings.
/// HR managers only see entries for their own branch; org admins and super admins see all.
/// </summary>
public record GetAuditLogsListQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Module = null,
    string? EntityName = null,
    Guid? EntityId = null,
    string? Action = null,
    Guid? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<PagedResult<AuditLogListDto>>>>;

public class GetAuditLogsListQueryHandler
    : IRequestHandler<GetAuditLogsListQuery, ErrorOr<GenericResponse<PagedResult<AuditLogListDto>>>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ApplicationDbContext _context;

    public GetAuditLogsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<AuditLogListDto>>>> Handle(
        GetAuditLogsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        // Branch scope: HR roles only see their own branch (or tenant-wide settings rows).
        var isSuperOrOrgAdmin = CurrentUser.IsSuperAdmin || CurrentUser.IsOrganizationAdmin;
        if (!isSuperOrOrgAdmin)
        {
            var branchId = CurrentUser.BranchId;
            query = query.Where(a => a.BranchId == branchId || a.BranchId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
            query = query.Where(a => a.Module == request.Module);

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.FromDate.HasValue)
        {
            var from = new DateTimeOffset(request.FromDate.Value.Date, TimeSpan.Zero);
            query = query.Where(a => a.Timestamp >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = new DateTimeOffset(request.ToDate.Value.Date.AddDays(1), TimeSpan.Zero);
            query = query.Where(a => a.Timestamp < to);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a =>
                (a.EntityDisplay != null && a.EntityDisplay.Contains(term)) ||
                (a.UserName != null && a.UserName.Contains(term)) ||
                a.EntityName.Contains(term) ||
                (a.ChangesJson != null && a.ChangesJson.Contains(term)));
        }

        var paged = await query
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new
            {
                a.Id,
                a.Module,
                a.EntityName,
                a.EntityId,
                a.EntityDisplay,
                a.Action,
                a.ChangedFieldsCount,
                a.ChangesJson,
                a.UserId,
                a.UserName,
                a.IpAddress,
                a.BranchId,
                a.Timestamp
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(a => new AuditLogListDto
        {
            Id = a.Id,
            Module = a.Module,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            EntityDisplay = a.EntityDisplay,
            Action = a.Action,
            ChangedFieldsCount = a.ChangedFieldsCount,
            Changes = ParseChanges(a.ChangesJson),
            UserId = a.UserId,
            UserName = a.UserName,
            IpAddress = a.IpAddress,
            BranchId = a.BranchId,
            Timestamp = a.Timestamp
        }).ToList();

        var result = PagedResult<AuditLogListDto>.Create(items, paged.TotalCount, paged.PageNumber, paged.PageSize);

        return new GenericResponse<PagedResult<AuditLogListDto>>
        {
            Success = true,
            Message = "Audit logs retrieved successfully",
            Data = result
        };
    }

    private static List<AuditFieldChangeDto> ParseChanges(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<AuditFieldChangeDto>();

        try
        {
            return JsonSerializer.Deserialize<List<AuditFieldChangeDto>>(json, JsonOptions) ?? new List<AuditFieldChangeDto>();
        }
        catch
        {
            return new List<AuditFieldChangeDto>();
        }
    }
}
