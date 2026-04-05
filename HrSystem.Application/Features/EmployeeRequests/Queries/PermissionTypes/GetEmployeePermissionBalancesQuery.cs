using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;

/// <summary>
/// Query to get all permission balances for an employee
/// </summary>
public record GetEmployeePermissionBalancesQuery(
    Guid EmployeeId,
    int? Year = null,
    int? Month = null
) : IRequest<ErrorOr<GenericResponse<List<EmployeePermissionBalanceDto>>>>;

public record EmployeePermissionBalanceDto
{
    public Guid PermissionTypeId { get; init; }
    public string PermissionTypeName { get; init; } = string.Empty;
    public string PermissionTypeNameAr { get; init; } = string.Empty;
    public decimal? MaxHoursPerMonth { get; init; }
    public decimal UsedHours { get; init; }
    public decimal RemainingHours => MaxHoursPerMonth.HasValue 
        ? Math.Max(0, MaxHoursPerMonth.Value - UsedHours) 
        : decimal.MaxValue;
    public bool HasLimit => MaxHoursPerMonth.HasValue;
    public string? Notes { get; init; }
}

public class GetEmployeePermissionBalancesQueryHandler 
    : IRequestHandler<GetEmployeePermissionBalancesQuery, ErrorOr<GenericResponse<List<EmployeePermissionBalanceDto>>>>
{
    private static readonly EmployeeRequestStatus[] CountableStatuses =
    {
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved,
        EmployeeRequestStatus.Approved,
        EmployeeRequestStatus.Completed
    };

    private readonly ApplicationDbContext _context;

    public GetEmployeePermissionBalancesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeePermissionBalanceDto>>>> Handle(
        GetEmployeePermissionBalancesQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get active permission types
        var permissionTypes = await _context.PermissionTypes
            .AsNoTracking()
            .Where(pt => pt.IsActive)
            .ToListAsync(cancellationToken);

        var result = new List<EmployeePermissionBalanceDto>();

        foreach (var permissionType in permissionTypes)
        {
            // Get employee-specific limit or fall back to permission type default
            var employeeLimit = await _context.EmployeePermissionLimits
                .AsNoTracking()
                .Where(epl => epl.EmployeeId == request.EmployeeId 
                           && epl.PermissionTypeId == permissionType.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var maxHours = employeeLimit?.MaxHoursPerMonth ?? permissionType.DefaultMonthlyHours;

            // Get used hours for the month
            var usedHours = await _context.PermissionRequestDetails
                .AsNoTracking()
                .Where(pd => pd.PermissionTypeId == permissionType.Id
                            && pd.EmployeeRequest.EmployeeId == request.EmployeeId
                            && pd.PermissionDate >= startOfMonth
                            && pd.PermissionDate <= endOfMonth
                            && CountableStatuses.Contains(pd.EmployeeRequest.Status))
                .SumAsync(pd => pd.TotalHours, cancellationToken);

            result.Add(new EmployeePermissionBalanceDto
            {
                PermissionTypeId = permissionType.Id,
                PermissionTypeName = permissionType.NameEn,
                PermissionTypeNameAr = permissionType.NameAr,
                MaxHoursPerMonth = maxHours,
                UsedHours = usedHours,
                Notes = employeeLimit?.Notes
            });
        }

        return GenericResponse<List<EmployeePermissionBalanceDto>>.SuccessResult(result);
    }
}
