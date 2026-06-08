using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;

/// <summary>
/// Query to get payslips list for the currently logged-in user
/// </summary>
public record GetMyPayslipsQuery(int? Year = null, int PageNumber = 1, int PageSize = 10)
    : IRequest<ErrorOr<GenericResponse<PagedResult<MyPayslipListItemDto>>>>;

public class GetMyPayslipsQueryHandler : IRequestHandler<GetMyPayslipsQuery, ErrorOr<GenericResponse<PagedResult<MyPayslipListItemDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyPayslipsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<MyPayslipListItemDto>>>> Handle(
        GetMyPayslipsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Payslip.Unauthorized", "Current user is not linked to an employee");
        }

        var year = request.Year ?? DateTime.UtcNow.Year;

        var payslipsQuery = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => !p.IsDeleted && p.EmployeeId == employeeId.Value && p.PayrollCycle.Year == year)
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .Select(p => new MyPayslipListItemDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                IsPaid = p.IsPaid,
                PdfFileUrl = p.PdfFileUrl,
                GeneratedDate = p.GeneratedDate
            });

        var pagedResult = await payslipsQuery.ToPagedResultAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new GenericResponse<PagedResult<MyPayslipListItemDto>>
        {
            Success = true,
            Message = "Payslips retrieved successfully",
            Data = pagedResult
        };
    }
}
