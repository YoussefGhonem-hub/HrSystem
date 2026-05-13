using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
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
        var periodStart = new DateTime(request.Year, request.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var periodEnd = new DateTime(request.Year, request.Month, daysInMonth);

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
        var statistics = await CalculateStatistics(
            request.Month,
            request.Year,
            currentCycle,
            lastMonthCycle,
            periodStart,
            periodEnd,
            cancellationToken);

        // Determine branch scope for HR roles
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Get payroll overview grid data
        var query = _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.Employee)
                .ThenInclude(e => e.Branch)
            .Include(p => p.PayrollCycle)
            .Where(p => p.PayrollCycle.Month == request.Month &&
                        p.PayrollCycle.Year == request.Year &&
                        p.Employee.HiringDate.HasValue &&
                        p.Employee.HiringDate.Value.Date <= periodEnd &&
                        (!p.Employee.TerminationDate.HasValue || p.Employee.TerminationDate.Value.Date >= periodStart))
            .AsQueryable();

        // Apply branch scope for HR managers
        if (branchId.HasValue)
        {
            query = query.Where(p => p.Employee.BranchId == branchId.Value);
        }

        // Apply filters
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(p => p.Employee.DepartmentId == request.DepartmentId.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(p => p.Employee.BranchId == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Employee.EmployeeCode.ToLower().Contains(searchTerm) ||
                p.Employee.FirstNameEn.ToLower().Contains(searchTerm) ||
                p.Employee.LastNameEn.ToLower().Contains(searchTerm) ||
                p.Employee.FirstNameAr.Contains(searchTerm) ||
                p.Employee.LastNameAr.Contains(searchTerm) ||
                p.Employee.Department.NameEn.ToLower().Contains(searchTerm) ||
                p.Employee.Department.NameAr.Contains(searchTerm));
        }

        var projectedRows = await query
            .Select(p => new
            {
                p.EmployeeId,
                p.Employee.EmployeeCode,
                EmployeeName = p.Employee.FullNameEn,
                DepartmentName = p.Employee.Department != null ? p.Employee.Department.NameEn : "No Department",
                BranchName = p.Employee.Branch != null ? p.Employee.Branch.NameEn : "No Branch",
                p.BasicSalary,
                p.TotalAllowances,
                p.OvertimeAmount,
                p.GrossSalary,
                p.TotalDeductions,
                p.NetSalary,
                p.TotalWorkingDays,
                p.PaidDate,
                p.IsPaid,
                EmployeeHiringDate = p.Employee.HiringDate,
                EmployeeTerminationDate = p.Employee.TerminationDate
            })
            .ToListAsync(cancellationToken);

        var employeeData = projectedRows
            .Select(row => new PayrollOverviewDto
            {
                EmployeeId = row.EmployeeId,
                EmployeeCode = row.EmployeeCode,
                EmployeeName = row.EmployeeName,
                DepartmentName = row.DepartmentName,
                BranchName = row.BranchName,
                GrossSalary = CalculateOverviewGrossSalary(
                    row.BasicSalary,
                    row.TotalAllowances,
                    row.OvertimeAmount,
                    row.GrossSalary,
                    row.TotalWorkingDays,
                    row.EmployeeHiringDate,
                    row.EmployeeTerminationDate,
                    periodStart,
                    periodEnd,
                    daysInMonth),
                TotalDeductions = row.TotalDeductions,
                NetSalary = row.NetSalary,
                Status = row.IsPaid ? "Paid" : "Pending",
                PaymentDate = row.PaidDate
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var normalizedStatus = request.Status.Trim().ToLower();
            employeeData = employeeData
                .Where(x => x.Status.ToLower() == normalizedStatus)
                .ToList();
        }

        var sortedData = ApplySorting(employeeData.AsQueryable(), request.SortBy, request.SortDescending);
        var totalCount = sortedData.Count();

        var items = sortedData
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

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

    private static decimal CalculateOverviewGrossSalary(
        decimal basicSalary,
        decimal totalAllowances,
        decimal overtimeAmount,
        decimal grossSalary,
        int totalWorkingDays,
        DateTime? hiringDate,
        DateTime? terminationDate,
        DateTime periodStart,
        DateTime periodEnd,
        int daysInMonth)
    {
        var overlapStart = hiringDate.HasValue && hiringDate.Value.Date > periodStart
            ? hiringDate.Value.Date
            : periodStart;
        var overlapEnd = terminationDate.HasValue && terminationDate.Value.Date < periodEnd
            ? terminationDate.Value.Date
            : periodEnd;

        if (overlapEnd < overlapStart)
            return 0m;

        var payableDays = (overlapEnd - overlapStart).Days + 1;
        var sourceWorkingDays = totalWorkingDays > 0 ? totalWorkingDays : daysInMonth;

        // Backward-compatibility for previously generated full-month payslips:
        // if stored working days exceed expected overlap, scale recurring components.
        if (sourceWorkingDays > payableDays)
        {
            var recurringComponent = basicSalary + totalAllowances;
            var proratedRecurring = Math.Round(recurringComponent * (payableDays / (decimal)sourceWorkingDays), 2);
            return Math.Round(proratedRecurring + overtimeAmount, 2);
        }

        return grossSalary;
    }

    private async Task<PayrollStatisticsDto> CalculateStatistics(
        int month,
        int year,
        Domain.Entities.Payroll.PayrollCycle? currentCycle,
        Domain.Entities.Payroll.PayrollCycle? lastMonthCycle,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        // Determine branch scope
        var isSuperOrOrgAdmin2 = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var statsBranchId = isSuperOrOrgAdmin2 ? (Guid?)null : CurrentUser.BranchId;

        // Compute totals from branch-filtered payslips instead of cycle-level aggregates
        decimal currentMonthTotal;
        decimal lastMonthTotal;

        if (statsBranchId.HasValue)
        {
            currentMonthTotal = currentCycle != null
                ? await _context.Payslips
                    .Where(p => p.PayrollCycleId == currentCycle.Id &&
                                p.Employee.BranchId == statsBranchId.Value &&
                                p.Employee.HiringDate.HasValue &&
                                p.Employee.HiringDate.Value.Date <= periodEnd &&
                                (!p.Employee.TerminationDate.HasValue || p.Employee.TerminationDate.Value.Date >= periodStart))
                    .SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0
                : 0;

            var lastMonth2 = month == 1 ? 12 : month - 1;
            var lastYear2 = month == 1 ? year - 1 : year;
            lastMonthTotal = lastMonthCycle != null
                ? await _context.Payslips
                    .Where(p => p.PayrollCycleId == lastMonthCycle.Id &&
                                p.Employee.BranchId == statsBranchId.Value)
                    .SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0
                : 0;
        }
        else
        {
            currentMonthTotal = currentCycle != null
                ? await _context.Payslips
                    .Where(p => p.PayrollCycleId == currentCycle.Id &&
                                p.Employee.HiringDate.HasValue &&
                                p.Employee.HiringDate.Value.Date <= periodEnd &&
                                (!p.Employee.TerminationDate.HasValue || p.Employee.TerminationDate.Value.Date >= periodStart))
                    .SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0
                : 0;

            lastMonthTotal = lastMonthCycle != null
                ? await _context.Payslips
                    .Where(p => p.PayrollCycleId == lastMonthCycle.Id)
                    .SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0
                : 0;
        }

        var changePercentage = lastMonthTotal > 0
            ? ((currentMonthTotal - lastMonthTotal) / lastMonthTotal) * 100
            : 0;

        // Get active employees with their contract types (scoped to branch)
        var employeesQuery = _context.Employees
            .Include(e => e.ContractType)
            .Where(e => e.Status.NameEn == "Active" &&
                        e.HiringDate.HasValue &&
                        e.HiringDate.Value.Date <= periodEnd &&
                        (!e.TerminationDate.HasValue || e.TerminationDate.Value.Date >= periodStart));

        if (statsBranchId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.BranchId == statsBranchId.Value);
        }

        var activeEmployees = await employeesQuery.ToListAsync(cancellationToken);

        var totalEmployees = activeEmployees.Count;
        
        // Try to identify full-time vs contract based on ContractType name
        var fullTimeCount = activeEmployees.Count(e => 
            e.ContractType != null && 
            (e.ContractType.NameEn.Contains("Full", StringComparison.OrdinalIgnoreCase) ||
             e.ContractType.NameEn.Contains("Permanent", StringComparison.OrdinalIgnoreCase)));
        
        var contractCount = totalEmployees - fullTimeCount;

        // Get pending actions (unpaid payslips, scoped to branch)
        var pendingQuery = _context.Payslips
            .Where(p => p.PayrollCycle.Month == month && 
                       p.PayrollCycle.Year == year && 
                       !p.IsPaid &&
                       p.Employee.HiringDate.HasValue &&
                       p.Employee.HiringDate.Value.Date <= periodEnd &&
                       (!p.Employee.TerminationDate.HasValue || p.Employee.TerminationDate.Value.Date >= periodStart));

        if (statsBranchId.HasValue)
        {
            pendingQuery = pendingQuery.Where(p => p.Employee.BranchId == statsBranchId.Value);
        }

        var pendingCount = await pendingQuery.CountAsync(cancellationToken);

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
            "employeecode" => descending ? query.OrderByDescending(x => x.EmployeeCode) : query.OrderBy(x => x.EmployeeCode),
            "employeename" => descending ? query.OrderByDescending(x => x.EmployeeName) : query.OrderBy(x => x.EmployeeName),
            "departmentname" => descending ? query.OrderByDescending(x => x.DepartmentName) : query.OrderBy(x => x.DepartmentName),
            "branchname" => descending ? query.OrderByDescending(x => x.BranchName) : query.OrderBy(x => x.BranchName),
            "grosssalary" => descending ? query.OrderByDescending(x => x.GrossSalary) : query.OrderBy(x => x.GrossSalary),
            "totaldeductions" => descending ? query.OrderByDescending(x => x.TotalDeductions) : query.OrderBy(x => x.TotalDeductions),
            "netsalary" => descending ? query.OrderByDescending(x => x.NetSalary) : query.OrderBy(x => x.NetSalary),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => query.OrderBy(x => x.EmployeeCode)
        };
    }
}
