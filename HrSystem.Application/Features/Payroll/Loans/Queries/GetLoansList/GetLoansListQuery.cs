using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Queries.GetLoansList;

public record GetLoansListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? EmployeeId = null,
    bool? IsActive = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<LoanListDto>>>>;

public class GetLoansListQueryHandler : IRequestHandler<GetLoansListQuery, ErrorOr<GenericResponse<PagedResult<LoanListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetLoansListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<LoanListDto>>>> Handle(
        GetLoansListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Loans
            .Include(l => l.Employee)
            .Where(l => !l.IsDeleted)
            .AsQueryable();

        // Apply branch scope for HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        if (branchId.HasValue)
        {
            query = query.Where(l => l.Employee.BranchId == branchId.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == request.EmployeeId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(l =>
                l.LoanName.Contains(term) ||
                l.Employee.EmployeeCode.Contains(term) ||
                l.Employee.FullNameEn.Contains(term) ||
                l.Employee.FullNameAr.Contains(term));
        }

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var listQuery = query.Select(l => new LoanListDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeCode = l.Employee.EmployeeCode,
            EmployeeNameEn = l.Employee.FullNameEn,
            EmployeeNameAr = l.Employee.FullNameAr,
            LoanName = l.LoanName,
            TotalAmount = l.TotalAmount,
            RemainingAmount = l.RemainingAmount,
            MonthlyDeduction = l.MonthlyDeduction,
            InstallmentMonths = l.InstallmentMonths,
            StartDate = l.StartDate,
            // Compute EndDate from StartDate + InstallmentMonths when not explicitly stored
            EndDate = l.EndDate ?? l.StartDate.AddMonths(l.InstallmentMonths),
            IsActive = l.IsActive
        });

        var pagedResult = await listQuery.ToPagedResultAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new GenericResponse<PagedResult<LoanListDto>>
        {
            Success = true,
            Message = "Loans retrieved successfully",
            Data = pagedResult
        };
    }

    private static IQueryable<Domain.Entities.Payroll.Loan> ApplySorting(
        IQueryable<Domain.Entities.Payroll.Loan> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(l => l.CreatedDate);
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "loanname" => sortDescending ? query.OrderByDescending(l => l.LoanName) : query.OrderBy(l => l.LoanName),
            "totalamount" => sortDescending ? query.OrderByDescending(l => l.TotalAmount) : query.OrderBy(l => l.TotalAmount),
            "remainingamount" => sortDescending ? query.OrderByDescending(l => l.RemainingAmount) : query.OrderBy(l => l.RemainingAmount),
            "monthlydeduction" => sortDescending ? query.OrderByDescending(l => l.MonthlyDeduction) : query.OrderBy(l => l.MonthlyDeduction),
            "isactive" => sortDescending ? query.OrderByDescending(l => l.IsActive) : query.OrderBy(l => l.IsActive),
            _ => sortDescending ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate)
        };
    }
}
