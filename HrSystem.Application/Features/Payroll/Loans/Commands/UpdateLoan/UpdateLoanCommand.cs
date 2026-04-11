using ErrorOr;
using HrSystem.Application.Features.Payroll.Loans.Queries.GetLoanById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Loans.Commands.UpdateLoan;

public record UpdateLoanCommand(
    Guid Id,
    Guid EmployeeId,
    string LoanName,
    decimal TotalAmount,
    decimal RemainingAmount,
    decimal MonthlyDeduction,
    int InstallmentMonths,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<LoanDto>>>;

public class UpdateLoanCommandHandler : IRequestHandler<UpdateLoanCommand, ErrorOr<GenericResponse<LoanDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateLoanCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LoanDto>>> Handle(
        UpdateLoanCommand request,
        CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => !l.IsDeleted && l.Id == request.Id, cancellationToken);

        if (loan == null)
        {
            return Error.NotFound("Loan.NotFound", "Loan not found");
        }

        // Verify branch scope for HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        if (!isSuperOrOrgAdmin && CurrentUser.BranchId.HasValue && loan.Employee?.BranchId != CurrentUser.BranchId)
        {
            return Error.Forbidden("Loan.BranchMismatch", "You can only update loans for employees in your branch");
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        loan.EmployeeId = request.EmployeeId;
        loan.LoanName = request.LoanName;
        loan.TotalAmount = request.TotalAmount;
        loan.RemainingAmount = request.RemainingAmount;
        loan.MonthlyDeduction = request.MonthlyDeduction;
        loan.InstallmentMonths = request.InstallmentMonths;
        loan.StartDate = request.StartDate;
        loan.EndDate = request.EndDate;
        loan.IsActive = request.IsActive;
        loan.Notes = request.Notes;
        loan.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.Loans
            .Include(l => l.Employee)
            .FirstAsync(l => l.Id == loan.Id, cancellationToken);

        var dto = new LoanDto
        {
            Id = updated.Id,
            EmployeeId = updated.EmployeeId,
            EmployeeCode = updated.Employee.EmployeeCode,
            EmployeeNameEn = updated.Employee.FullNameEn,
            EmployeeNameAr = updated.Employee.FullNameAr,
            LoanName = updated.LoanName,
            TotalAmount = updated.TotalAmount,
            RemainingAmount = updated.RemainingAmount,
            MonthlyDeduction = updated.MonthlyDeduction,
            InstallmentMonths = updated.InstallmentMonths,
            StartDate = updated.StartDate,
            EndDate = updated.EndDate ?? updated.StartDate.AddMonths(updated.InstallmentMonths),
            IsActive = updated.IsActive,
            Notes = updated.Notes
        };

        return new GenericResponse<LoanDto>
        {
            Success = true,
            Message = "Loan updated successfully",
            Data = dto
        };
    }
}
