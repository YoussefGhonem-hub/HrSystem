using ErrorOr;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public class ConfigureEmployeePayrollCommandHandler : IRequestHandler<ConfigureEmployeePayrollCommand, ErrorOr<GenericResponse<EmployeePayrollConfigurationDto>>>
{
    private readonly ApplicationDbContext _context;

    public ConfigureEmployeePayrollCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeePayrollConfigurationDto>>> Handle(
        ConfigureEmployeePayrollCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("PayrollConfiguration.Unauthorized", "Not allowed to configure payroll for this employee");
            }
        }

        var tenantId = employee.TenantId != Guid.Empty
            ? employee.TenantId
            : CurrentUser.OrganizationId ?? Guid.Empty;

        if (tenantId == Guid.Empty)
        {
            return Error.Failure("PayrollConfiguration.InvalidTenant", "Unable to determine organization context for salary configuration");
        }

        var branchId = employee.BranchId;
        var currency = !string.IsNullOrWhiteSpace(request.Currency)
            ? request.Currency!.Trim().ToUpperInvariant()
            : employee.Branch?.Currency;

        if (string.IsNullOrWhiteSpace(currency))
        {
            currency = await _context.Organizations
                .Where(o => o.Id == tenantId)
                .Select(o => o.Currency)
                .FirstOrDefaultAsync(cancellationToken);
        }

        currency ??= "EGP";

        var allowanceTypeIds = request.Allowances?.Select(a => a.AllowanceTypeId).Distinct().ToList() ?? new List<Guid>();
        if (allowanceTypeIds.Count > 0)
        {
            var existingAllowanceIds = await _context.AllowanceTypes
                .Where(a => allowanceTypeIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            var missingAllowanceIds = allowanceTypeIds.Except(existingAllowanceIds).ToList();
            if (missingAllowanceIds.Count > 0)
            {
                return Error.NotFound("PayrollConfiguration.AllowanceTypeNotFound", "One or more allowance types do not exist");
            }
        }

        var deductionTypeIds = request.Deductions?.Select(d => d.DeductionTypeId).Distinct().ToList() ?? new List<Guid>();
        if (deductionTypeIds.Count > 0)
        {
            var existingDeductionIds = await _context.DeductionTypes
                .Where(d => deductionTypeIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);

            var missingDeductionIds = deductionTypeIds.Except(existingDeductionIds).ToList();
            if (missingDeductionIds.Count > 0)
            {
                return Error.NotFound("PayrollConfiguration.DeductionTypeNotFound", "One or more deduction types do not exist");
            }
        }

        var currentSalary = await _context.Salaries
            .Where(s => !s.IsDeleted && s.EmployeeId == request.EmployeeId && s.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentSalary != null)
        {
            currentSalary.IsCurrent = false;
            currentSalary.EndDate = request.EffectiveDate > currentSalary.EffectiveDate
                ? request.EffectiveDate.AddDays(-1)
                : request.EffectiveDate;
            currentSalary.MarkAsModified(CurrentUser.Id ?? Guid.Empty);
        }

        var salary = new Salary
        {
            EmployeeId = request.EmployeeId,
            BasicSalary = request.BasicSalary,
            EffectiveDate = request.EffectiveDate,
            Notes = request.Notes,
            IsCurrent = true,
            Currency = currency,
            IsSocialInsuranceEnabled = request.IncludeSocialInsurance,
            SocialInsuranceEmployeeRate = request.SocialInsuranceEmployeeRate,
            SocialInsuranceEmployerRate = request.SocialInsuranceEmployerRate,
            PaymentMethod = request.PaymentMethod,
            BankName = request.BankInfo?.BankName,
            BankBranch = request.BankInfo?.BankBranch,
            BankAccountNumber = request.BankInfo?.AccountNumber,
            BankIban = request.BankInfo?.Iban,
            BankSwiftCode = request.BankInfo?.SwiftCode,
            TenantId = tenantId,
            BranchId = branchId
        };

        var currentUserId = CurrentUser.Id ?? Guid.Empty;
        salary.MarkAsCreated(currentUserId);

        if (request.Allowances is { Count: > 0 })
        {
            foreach (var allowance in request.Allowances)
            {
                var allowanceEntity = new SalaryAllowance
                {
                    AllowanceTypeId = allowance.AllowanceTypeId,
                    Amount = allowance.Amount,
                    IsPercentage = allowance.IsPercentage,
                    PercentageValue = allowance.IsPercentage ? allowance.PercentageValue : null,
                    TenantId = tenantId,
                    BranchId = branchId
                };
                allowanceEntity.MarkAsCreated(currentUserId);
                salary.Allowances.Add(allowanceEntity);
            }
        }

        if (request.Deductions is { Count: > 0 })
        {
            foreach (var deduction in request.Deductions)
            {
                var deductionEntity = new SalaryDeduction
                {
                    DeductionTypeId = deduction.DeductionTypeId,
                    Amount = deduction.Amount,
                    IsPercentage = deduction.IsPercentage,
                    PercentageValue = deduction.IsPercentage ? deduction.PercentageValue : null,
                    TenantId = tenantId,
                    BranchId = branchId
                };
                deductionEntity.MarkAsCreated(currentUserId);
                salary.Deductions.Add(deductionEntity);
            }
        }

        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new EmployeePayrollConfigurationDto
        {
            SalaryId = salary.Id,
            EmployeeId = salary.EmployeeId,
            BasicSalary = salary.BasicSalary,
            EffectiveDate = salary.EffectiveDate,
            EndDate = salary.EndDate,
            IsCurrent = salary.IsCurrent,
            Currency = salary.Currency,
            IncludeSocialInsurance = salary.IsSocialInsuranceEnabled,
            SocialInsuranceEmployeeRate = salary.SocialInsuranceEmployeeRate,
            SocialInsuranceEmployerRate = salary.SocialInsuranceEmployerRate,
            PaymentMethod = salary.PaymentMethod,
            Notes = salary.Notes,
            BankInfo = new PayrollBankInfoDto
            {
                BankName = salary.BankName,
                BankBranch = salary.BankBranch,
                AccountNumber = salary.BankAccountNumber,
                Iban = salary.BankIban,
                SwiftCode = salary.BankSwiftCode
            },
            Allowances = salary.Allowances.Select(a => new PayrollAllowanceDto
            {
                Id = a.Id,
                AllowanceTypeId = a.AllowanceTypeId,
                Amount = a.Amount,
                IsPercentage = a.IsPercentage,
                PercentageValue = a.PercentageValue
            }).ToList(),
            Deductions = salary.Deductions.Select(d => new PayrollDeductionDto
            {
                Id = d.Id,
                DeductionTypeId = d.DeductionTypeId,
                Amount = d.Amount,
                IsPercentage = d.IsPercentage,
                PercentageValue = d.PercentageValue
            }).ToList()
        };

        return new GenericResponse<EmployeePayrollConfigurationDto>
        {
            Success = true,
            Message = "Employee payroll configuration saved successfully",
            Data = response
        };
    }
}
