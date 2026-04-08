using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;

public class GetPayslipsWithStatisticsQueryHandler : IRequestHandler<GetPayslipsWithStatisticsQuery, ErrorOr<GenericResponse<PayslipsResponseDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayslipsWithStatisticsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayslipsResponseDto>>> Handle(
        GetPayslipsWithStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        // Determine branch scope for HR roles
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Calculate statistics (scoped to branch)
        var statistics = await CalculatePayslipStatistics(request.Month, request.Year, branchId, cancellationToken);

        // Build query
        var query = _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.PayrollCycle)
            .Where(p => p.PayrollCycle.Month == request.Month && p.PayrollCycle.Year == request.Year)
            .AsQueryable();

        // Apply branch scope for HR managers
        if (branchId.HasValue)
        {
            query = query.Where(p => p.Employee.BranchId == branchId.Value);
        }

        // Apply filters
        if (request.EmployeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == request.EmployeeId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(p => p.Employee.DepartmentId == request.DepartmentId.Value);
        }

        if (request.IsPaid.HasValue)
        {
            query = query.Where(p => p.IsPaid == request.IsPaid.Value);
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
                p.PayslipNumber.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated data
        var payslips = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PayslipListDto
            {
                Id = p.Id,
                EmployeeCode = p.Employee.EmployeeCode,
                EmployeeNameAr = p.Employee.FullNameAr,
                EmployeeNameEn = p.Employee.FullNameEn,
                DepartmentName = p.Employee.Department != null ? p.Employee.Department.NameEn : "No Department",
                PayslipNumber = p.PayslipNumber,
                BasicSalary = p.BasicSalary,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Month = p.PayrollCycle.Month,
                Year = p.PayrollCycle.Year,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.PayrollCycle.Month),
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate,
                Status = p.IsPaid ? "Paid" : (p.GeneratedDate.HasValue ? "Generated" : "Draft"),
                PdfFileUrl = p.PdfFileUrl,
                GeneratedDate = p.GeneratedDate
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<PayslipListDto>.Create(
            payslips,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        var response = new PayslipsResponseDto
        {
            Statistics = statistics,
            PayslipsData = pagedResult
        };

        return GenericResponse<PayslipsResponseDto>.SuccessResult(response, "Payslips retrieved successfully");
    }

    private async Task<PayslipStatisticsDto> CalculatePayslipStatistics(
        int month,
        int year,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var payslipQuery = _context.Payslips
            .Where(p => p.PayrollCycle.Month == month && p.PayrollCycle.Year == year);

        if (branchId.HasValue)
        {
            payslipQuery = payslipQuery.Where(p => p.Employee.BranchId == branchId.Value);
        }

        var payslips = await payslipQuery
            .Select(p => new
            {
                p.IsPaid,
                p.GeneratedDate,
                p.NetSalary,
                p.TotalDeductions,
                p.IncomeTax,
                p.SocialInsuranceEmployee
            })
            .ToListAsync(cancellationToken);

        var totalPayslips = payslips.Count;
        var sentPayslips = payslips.Count(p => p.IsPaid);
        var pendingPayslips = payslips.Count(p => !p.IsPaid);

        var totalNetSalary = payslips.Sum(p => p.NetSalary);
        var averageSalary = payslips.Any() ? payslips.Average(p => p.NetSalary) : 0;
        var medianSalary = CalculateMedian(payslips.Select(p => p.NetSalary).ToList());

        var totalDeductions = payslips.Sum(p => p.TotalDeductions);
        var taxDeductions = payslips.Sum(p => p.IncomeTax);
        var insuranceDeductions = payslips.Sum(p => p.SocialInsuranceEmployee);
        var otherDeductions = totalDeductions - taxDeductions - insuranceDeductions;

        return new PayslipStatisticsDto
        {
            TotalPayslips = totalPayslips,
            SentPayslips = sentPayslips,
            PendingPayslips = pendingPayslips,
            TotalNetSalary = Math.Round(totalNetSalary, 2),
            AverageNetSalary = Math.Round(averageSalary, 2),
            MedianSalary = Math.Round(medianSalary, 2),
            TotalDeductions = Math.Round(totalDeductions, 2),
            TaxDeductions = Math.Round(taxDeductions, 2),
            InsuranceDeductions = Math.Round(insuranceDeductions, 2),
            OtherDeductions = Math.Round(otherDeductions, 2)
        };
    }

    private decimal CalculateMedian(List<decimal> values)
    {
        if (!values.Any()) return 0;

        var sortedValues = values.OrderBy(v => v).ToList();
        var count = sortedValues.Count;

        if (count % 2 == 0)
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2;
        }
        else
        {
            return sortedValues[count / 2];
        }
    }

    private IQueryable<Domain.Entities.Payroll.Payslip> ApplySorting(
        IQueryable<Domain.Entities.Payroll.Payslip> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "employeecode" => descending ? query.OrderByDescending(x => x.Employee.EmployeeCode) : query.OrderBy(x => x.Employee.EmployeeCode),
            "employeename" => descending ? query.OrderByDescending(x => x.Employee.FirstNameEn) : query.OrderBy(x => x.Employee.FirstNameEn),
            "department" => descending ? query.OrderByDescending(x => x.Employee.Department.NameEn) : query.OrderBy(x => x.Employee.Department.NameEn),
            "grosssalary" => descending ? query.OrderByDescending(x => x.GrossSalary) : query.OrderBy(x => x.GrossSalary),
            "netsalary" => descending ? query.OrderByDescending(x => x.NetSalary) : query.OrderBy(x => x.NetSalary),
            "status" => descending ? query.OrderByDescending(x => x.IsPaid) : query.OrderBy(x => x.IsPaid),
            "generateddate" => descending ? query.OrderByDescending(x => x.GeneratedDate) : query.OrderBy(x => x.GeneratedDate),
            _ => query.OrderBy(x => x.Employee.EmployeeCode)
        };
    }
}
