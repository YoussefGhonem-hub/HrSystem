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

        // Ensure referenced allowance/deduction types exist; create missing ones to allow seamless configuration
        // This avoids failing the request when client uses new type IDs.

        var currentSalary = await _context.Salaries
            .Include(s => s.Allowances.Where(a => !a.IsDeleted))
            .Include(s => s.Deductions.Where(d => !d.IsDeleted))
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
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {

            throw;
        }

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
            Allowances = salary.Allowances
                .Where(a => !a.IsDeleted)
                .Select(a => new PayrollAllowanceDto
                {
                    Id = a.Id,
                    NameAr = a.NameAr,
                    NameEn = a.NameEn,
                    Description = a.Description,
                    IsTaxable = a.IsTaxable,
                    IsSubjectToInsurance = a.IsSubjectToInsurance,
                    Amount = a.Amount,
                    IsPercentage = a.IsPercentage,
                    PercentageValue = a.PercentageValue
                }).ToList(),
            Deductions = salary.Deductions
                .Where(d => !d.IsDeleted)
                .Select(d => new PayrollDeductionDto
                {
                    Id = d.Id,
                    NameAr = d.NameAr,
                    NameEn = d.NameEn,
                    Description = d.Description,
                    IsRecurring = d.IsRecurring,
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
        var existing = salary.Allowances.ToList();
        
        // Group by normalized name to handle duplicates - take the last one
        var desiredLookup = desired
            .GroupBy(a => a.NameEn.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var allowance in existing)
        {
            var key = allowance.NameEn.Trim().ToLowerInvariant();
            if (!desiredLookup.TryGetValue(key, out var match))
            {
                // Remove from collection instead of soft-delete to avoid concurrency issues
                salary.Allowances.Remove(allowance);
                _context.SalaryAllowances.Remove(allowance);
                continue;
            }

            allowance.NameAr = match.NameAr;
            allowance.NameEn = match.NameEn;
            allowance.Description = match.Description;
            allowance.IsTaxable = match.IsTaxable;
            allowance.IsSubjectToInsurance = match.IsSubjectToInsurance;
            allowance.Amount = match.Amount;
            allowance.IsPercentage = match.IsPercentage;
            allowance.PercentageValue = match.IsPercentage ? match.PercentageValue : null;
            allowance.MarkAsModified(currentUserId);
            desiredLookup.Remove(key);
        }

        foreach (var remaining in desiredLookup.Values)
        {
            var allowanceEntity = new SalaryAllowance
            {
                NameAr = remaining.NameAr,
                NameEn = remaining.NameEn,
                Description = remaining.Description,
                IsTaxable = remaining.IsTaxable,
                IsSubjectToInsurance = remaining.IsSubjectToInsurance,
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
        var existing = salary.Deductions.ToList();
        
        // Group by normalized name to handle duplicates - take the last one
        var desiredLookup = desired
            .GroupBy(d => d.NameEn.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var deduction in existing)
        {
            var key = deduction.NameEn.Trim().ToLowerInvariant();
            if (!desiredLookup.TryGetValue(key, out var match))
            {
                // Remove from collection instead of soft-delete to avoid concurrency issues
                salary.Deductions.Remove(deduction);
                _context.SalaryDeductions.Remove(deduction);
                continue;
            }

            deduction.NameAr = match.NameAr;
            deduction.NameEn = match.NameEn;
            deduction.Description = match.Description;
            deduction.IsRecurring = match.IsRecurring;
            deduction.Amount = match.Amount;
            deduction.IsPercentage = match.IsPercentage;
            deduction.PercentageValue = match.IsPercentage ? match.PercentageValue : null;
            deduction.MarkAsModified(currentUserId);
            desiredLookup.Remove(key);
        }

        foreach (var remaining in desiredLookup.Values)
        {
            var deductionEntity = new SalaryDeduction
            {
                NameAr = remaining.NameAr,
                NameEn = remaining.NameEn,
                Description = remaining.Description,
                IsRecurring = remaining.IsRecurring,
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
