using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;

/// <summary>
/// Query to get employee's monthly permission hours usage for a specific permission type
/// </summary>
public record GetEmployeeMonthlyPermissionHoursQuery(
    Guid EmployeeId,
    Guid PermissionTypeId,
    int Year,
    int Month
) : IRequest<ErrorOr<GenericResponse<EmployeeMonthlyPermissionHoursDto>>>;

public record EmployeeMonthlyPermissionHoursDto
{
    public Guid EmployeeId { get; init; }
    public Guid PermissionTypeId { get; init; }
    public string PermissionTypeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal? MaxHoursPerMonth { get; init; }
    public decimal ApprovedHours { get; init; }
    public decimal PendingHours { get; init; }
    public decimal TotalUsedHours => ApprovedHours + PendingHours;
    public decimal RemainingHours => MaxHoursPerMonth.HasValue 
        ? Math.Max(0, MaxHoursPerMonth.Value - TotalUsedHours) 
        : decimal.MaxValue;
    public bool HasMonthlyLimit => MaxHoursPerMonth.HasValue;
}

public class GetEmployeeMonthlyPermissionHoursQueryHandler 
    : IRequestHandler<GetEmployeeMonthlyPermissionHoursQuery, ErrorOr<GenericResponse<EmployeeMonthlyPermissionHoursDto>>>
{
    private static readonly EmployeeRequestStatus[] ApprovedStatuses =
    {
        EmployeeRequestStatus.Approved,
        EmployeeRequestStatus.Completed
    };

    private readonly ApplicationDbContext _context;

    public GetEmployeeMonthlyPermissionHoursQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeMonthlyPermissionHoursDto>>> Handle(
        GetEmployeeMonthlyPermissionHoursQuery request,
        CancellationToken cancellationToken)
    {
        var permissionType = await _context.PermissionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.Id == request.PermissionTypeId, cancellationToken);

        if (permissionType == null)
            return Error.NotFound(description: "Permission type not found.");

        var startOfMonth = new DateTime(request.Year, request.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get approved hours for the month
        var approvedHours = await _context.PermissionRequestDetails
            .AsNoTracking()
            .Where(pd => pd.PermissionTypeId == request.PermissionTypeId
                        && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                        && pd.PermissionDate >= startOfMonth
                        && pd.PermissionDate <= endOfMonth
                        && ApprovedStatuses.Contains(pd.EmployeeRequest.Status))
            .SumAsync(pd => pd.TotalHours, cancellationToken);

        // Get pending hours for the month
        var pendingHours = await _context.PermissionRequestDetails
            .AsNoTracking()
            .Where(pd => pd.PermissionTypeId == request.PermissionTypeId
                        && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                        && pd.PermissionDate >= startOfMonth
                        && pd.PermissionDate <= endOfMonth
                        && pd.EmployeeRequest.Status == EmployeeRequestStatus.Pending)
            .SumAsync(pd => pd.TotalHours, cancellationToken);

        var dto = new EmployeeMonthlyPermissionHoursDto
        {
            EmployeeId = request.EmployeeId,
            PermissionTypeId = request.PermissionTypeId,
            PermissionTypeName = permissionType.NameEn,
            Year = request.Year,
            Month = request.Month,
            MaxHoursPerMonth = null,
            ApprovedHours = approvedHours,
            PendingHours = pendingHours
        };

        return GenericResponse<EmployeeMonthlyPermissionHoursDto>.SuccessResult(dto);
    }
}
