using ErrorOr;
using HrSystem.Application.Features.Payroll.Loans.Queries.GetLoanById;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Commands.CreateLoan;

public record CreateLoanCommand(
    Guid EmployeeId,
    string LoanName,
    decimal TotalAmount,
    decimal MonthlyDeduction,
    int InstallmentMonths,
    DateTime StartDate,
    bool IsActive = true,
    DateTime? EndDate = null,
    string? Notes = null
) : IRequest<ErrorOr<GenericResponse<LoanDto>>>;

public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, ErrorOr<GenericResponse<LoanDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateLoanCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LoanDto>>> Handle(
        CreateLoanCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        // Verify branch scope for HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        if (!isSuperOrOrgAdmin && CurrentUser.BranchId.HasValue && employee.BranchId != CurrentUser.BranchId)
        {
            return Error.Forbidden("Loan.BranchMismatch", "You can only create loans for employees in your branch");
        }

        var loan = new Loan
        {
            EmployeeId = request.EmployeeId,
            LoanName = request.LoanName,
            TotalAmount = request.TotalAmount,
            RemainingAmount = request.TotalAmount,
            MonthlyDeduction = request.MonthlyDeduction,
            InstallmentMonths = request.InstallmentMonths,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            Notes = request.Notes,
            TenantId = Guid.Empty
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Loans
            .Include(l => l.Employee)
            .FirstAsync(l => l.Id == loan.Id, cancellationToken);

        var dto = new LoanDto
        {
            Id = created.Id,
            EmployeeId = created.EmployeeId,
            EmployeeCode = created.Employee.EmployeeCode,
            EmployeeNameEn = created.Employee.FullNameEn,
            EmployeeNameAr = created.Employee.FullNameAr,
            LoanName = created.LoanName,
            TotalAmount = created.TotalAmount,
            RemainingAmount = created.RemainingAmount,
            MonthlyDeduction = created.MonthlyDeduction,
            InstallmentMonths = created.InstallmentMonths,
            StartDate = created.StartDate,
            EndDate = created.EndDate,
            IsActive = created.IsActive,
            Notes = created.Notes
        };

        return new GenericResponse<LoanDto>
        {
            Success = true,
            Message = "Loan created successfully",
            Data = dto
        };
    }
}
