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
            .Include(s => s.Allowances)
            .Include(s => s.Deductions)
            .Where(s => !s.IsDeleted && s.EmployeeId == request.EmployeeId && s.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        var currentUserId = CurrentUser.Id ?? Guid.Empty;
        Salary salary;
        var updatingExistingRecord = currentSalary != null && currentSalary.EffectiveDate.Date == request.EffectiveDate.Date;

        if (updatingExistingRecord)
        {
            salary = currentSalary!;
            salary.BasicSalary = request.BasicSalary;
            salary.EffectiveDate = request.EffectiveDate;
            salary.EndDate = null;
            salary.Notes = request.Notes;
            salary.IsCurrent = true;
            salary.Currency = currency;
            salary.IsSocialInsuranceEnabled = request.IncludeSocialInsurance;
            salary.SocialInsuranceEmployeeRate = request.SocialInsuranceEmployeeRate;
            salary.SocialInsuranceEmployerRate = request.SocialInsuranceEmployerRate;
            salary.PaymentMethod = request.PaymentMethod;
            salary.BankName = request.BankInfo?.BankName;
            salary.BankBranch = request.BankInfo?.BankBranch;
            salary.BankAccountNumber = request.BankInfo?.AccountNumber;
            salary.BankIban = request.BankInfo?.Iban;
            salary.BankSwiftCode = request.BankInfo?.SwiftCode;
            salary.TenantId = tenantId;
            salary.BranchId = branchId;
            salary.MarkAsModified(currentUserId);
        }
        else
        {
            if (currentSalary != null)
            {
                currentSalary.IsCurrent = false;
                currentSalary.EndDate = request.EffectiveDate > currentSalary.EffectiveDate
                    ? request.EffectiveDate.AddDays(-1)
                    : request.EffectiveDate;
                currentSalary.MarkAsModified(currentUserId);
            }

            salary = new Salary
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

            salary.MarkAsCreated(currentUserId);
            _context.Salaries.Add(salary);
        }

        SyncSalaryAllowances(salary, request.Allowances, tenantId, branchId, currentUserId);
        SyncSalaryDeductions(salary, request.Deductions, tenantId, branchId, currentUserId);

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

    private void SyncSalaryAllowances(
        Salary salary,
        List<PayrollAllowancePayload>? payloads,
        Guid tenantId,
        Guid? branchId,
        Guid currentUserId)
    {
        var desired = payloads ?? new List<PayrollAllowancePayload>();
        var existing = salary.Allowances.Where(a => !a.IsDeleted).ToList();
        var desiredLookup = desired.ToDictionary(a => a.AllowanceTypeId, a => a);

        foreach (var allowance in existing)
        {
            if (!desiredLookup.TryGetValue(allowance.AllowanceTypeId, out var match))
            {
                salary.Allowances.Remove(allowance);
                _context.SalaryAllowances.Remove(allowance);
                continue;
            }

            allowance.Amount = match.Amount;
            allowance.IsPercentage = match.IsPercentage;
            allowance.PercentageValue = match.IsPercentage ? match.PercentageValue : null;
            allowance.MarkAsModified(currentUserId);
            desiredLookup.Remove(allowance.AllowanceTypeId);
        }

        foreach (var remaining in desiredLookup.Values)
        {
            var allowanceEntity = new SalaryAllowance
            {
                SalaryId = salary.Id,
                AllowanceTypeId = remaining.AllowanceTypeId,
                Amount = remaining.Amount,
                IsPercentage = remaining.IsPercentage,
                PercentageValue = remaining.IsPercentage ? remaining.PercentageValue : null,
                TenantId = tenantId,
                BranchId = branchId
            };
            allowanceEntity.MarkAsCreated(currentUserId);
            salary.Allowances.Add(allowanceEntity);
        }
    }

    private void SyncSalaryDeductions(
        Salary salary,
        List<PayrollDeductionPayload>? payloads,
        Guid tenantId,
        Guid? branchId,
        Guid currentUserId)
    {
        var desired = payloads ?? new List<PayrollDeductionPayload>();
        var existing = salary.Deductions.Where(d => !d.IsDeleted).ToList();
        var desiredLookup = desired.ToDictionary(d => d.DeductionTypeId, d => d);

        foreach (var deduction in existing)
        {
            if (!desiredLookup.TryGetValue(deduction.DeductionTypeId, out var match))
            {
                salary.Deductions.Remove(deduction);
                _context.SalaryDeductions.Remove(deduction);
                continue;
            }

            deduction.Amount = match.Amount;
            deduction.IsPercentage = match.IsPercentage;
            deduction.PercentageValue = match.IsPercentage ? match.PercentageValue : null;
            deduction.MarkAsModified(currentUserId);
            desiredLookup.Remove(deduction.DeductionTypeId);
        }

        foreach (var remaining in desiredLookup.Values)
        {
            var deductionEntity = new SalaryDeduction
            {
                SalaryId = salary.Id,
                DeductionTypeId = remaining.DeductionTypeId,
                Amount = remaining.Amount,
                IsPercentage = remaining.IsPercentage,
                PercentageValue = remaining.IsPercentage ? remaining.PercentageValue : null,
                TenantId = tenantId,
                BranchId = branchId
            };
            deductionEntity.MarkAsCreated(currentUserId);
            salary.Deductions.Add(deductionEntity);
        }
    }
}
