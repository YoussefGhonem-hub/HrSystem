using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;

public class GetPayrollOverviewQueryHandler : IRequestHandler<GetPayrollOverviewQuery, ErrorOr<GenericResponse<PayrollOverviewResponseDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayrollOverviewResponseDto>>> Handle(
        GetPayrollOverviewQuery request,
        CancellationToken cancellationToken)
    {
        // Get current month payroll cycle
        var currentCycle = await _context.PayrollCycles
            .Where(pc => pc.Month == request.Month && pc.Year == request.Year)
            .FirstOrDefaultAsync(cancellationToken);

        // Get last month for comparison
        var lastMonth = request.Month == 1 ? 12 : request.Month - 1;
        var lastYear = request.Month == 1 ? request.Year - 1 : request.Year;
        
        var lastMonthCycle = await _context.PayrollCycles
            .Where(pc => pc.Month == lastMonth && pc.Year == lastYear)
            .FirstOrDefaultAsync(cancellationToken);

        // Calculate statistics
        var statistics = await CalculateStatistics(request.Month, request.Year, currentCycle, lastMonthCycle, cancellationToken);

        // Get payroll overview grid data
        var query = _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.PayrollCycle)
            .Where(p => p.PayrollCycle.Month == request.Month && p.PayrollCycle.Year == request.Year)
            .AsQueryable();

        // Apply filters
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(p => p.Employee.DepartmentId == request.DepartmentId.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(p => p.Employee.Branch!.Id == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Employee.Department.NameEn.ToLower().Contains(searchTerm) ||
                p.Employee.Department.NameAr.Contains(searchTerm));
        }

        // Group by department
        var departmentGroupQuery = query
            .GroupBy(p => new
            {
                DepartmentId = p.Employee.DepartmentId,
                DepartmentName = p.Employee.Department.NameEn,
                Month = p.PayrollCycle.Month,
                Year = p.PayrollCycle.Year
            })
            .Select(g => new PayrollOverviewDto
            {
                DepartmentId = g.Key.DepartmentId ?? Guid.Empty,
                DepartmentName = g.Key.DepartmentName ?? "No Department",
                EmployeeCount = g.Count(),
                GrossSalary = g.Sum(p => p.GrossSalary),
                TotalDeductions = g.Sum(p => p.TotalDeductions),
                NetSalary = g.Sum(p => p.NetSalary),
                Status = g.Any(p => !p.IsPaid) ? "Pending" : "Processed",
                Month = g.Key.Month,
                Year = g.Key.Year
            });

        // Apply sorting
        departmentGroupQuery = ApplySorting(departmentGroupQuery, request.SortBy, request.SortDescending);

        // Get total count
        var totalCount = await departmentGroupQuery.CountAsync(cancellationToken);

        // Apply pagination
        var items = await departmentGroupQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<PayrollOverviewDto>.Create(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        var response = new PayrollOverviewResponseDto
        {
            Statistics = statistics,
            PayrollData = pagedResult
        };

        return GenericResponse<PayrollOverviewResponseDto>.SuccessResult(response, "Payroll overview retrieved successfully");
    }

    private async Task<PayrollStatisticsDto> CalculateStatistics(
        int month,
        int year,
        Domain.Entities.Payroll.PayrollCycle? currentCycle,
        Domain.Entities.Payroll.PayrollCycle? lastMonthCycle,
        CancellationToken cancellationToken)
    {
        var currentMonthTotal = currentCycle?.TotalNetSalary ?? 0;
        var lastMonthTotal = lastMonthCycle?.TotalNetSalary ?? 0;

        var changePercentage = lastMonthTotal > 0
            ? ((currentMonthTotal - lastMonthTotal) / lastMonthTotal) * 100
            : 0;

        // Get active employees with their contract types
        var activeEmployees = await _context.Employees
            .Include(e => e.ContractType)
            .Where(e => e.Status.NameEn == "Active")
            .ToListAsync(cancellationToken);

        var totalEmployees = activeEmployees.Count;
        
        // Try to identify full-time vs contract based on ContractType name
        var fullTimeCount = activeEmployees.Count(e => 
            e.ContractType != null && 
            (e.ContractType.NameEn.Contains("Full", StringComparison.OrdinalIgnoreCase) ||
             e.ContractType.NameEn.Contains("Permanent", StringComparison.OrdinalIgnoreCase)));
        
        var contractCount = totalEmployees - fullTimeCount;

        // Get pending actions (unpaid payslips)
        var pendingCount = await _context.Payslips
            .Where(p => p.PayrollCycle.Month == month && 
                       p.PayrollCycle.Year == year && 
                       !p.IsPaid)
            .CountAsync(cancellationToken);

        return new PayrollStatisticsDto
        {
            TotalMonthlyPayroll = currentMonthTotal,
            LastMonthPayroll = lastMonthTotal,
            PayrollChangePercentage = Math.Round(changePercentage, 2),
            TotalEmployees = totalEmployees,
            FullTimeEmployees = fullTimeCount,
            ContractEmployees = contractCount,
            PendingPayrollActions = pendingCount
        };
    }

    private IQueryable<PayrollOverviewDto> ApplySorting(
        IQueryable<PayrollOverviewDto> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "departmentname" => descending ? query.OrderByDescending(x => x.DepartmentName) : query.OrderBy(x => x.DepartmentName),
            "employeecount" => descending ? query.OrderByDescending(x => x.EmployeeCount) : query.OrderBy(x => x.EmployeeCount),
            "grosssalary" => descending ? query.OrderByDescending(x => x.GrossSalary) : query.OrderBy(x => x.GrossSalary),
            "netsalary" => descending ? query.OrderByDescending(x => x.NetSalary) : query.OrderBy(x => x.NetSalary),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => query.OrderBy(x => x.DepartmentName)
        };
    }
}
