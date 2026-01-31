using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

internal static class EmployeeCommandHelper
{
    private const string EmployeeCodePrefix = "EMP-";

    public static async Task<EmployeeDto> BuildEmployeeDtoAsync(
        ApplicationDbContext context,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .Include(e => e.ContractType)
            .Include(e => e.Status)
            .FirstAsync(e => e.Id == employeeId, cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstNameAr = employee.FirstNameAr,
            LastNameAr = employee.LastNameAr,
            FirstNameEn = employee.FirstNameEn,
            LastNameEn = employee.LastNameEn,
            FullNameAr = employee.FullNameAr,
            FullNameEn = employee.FullNameEn,
            NationalId = employee.NationalId,
            PassportNumber = employee.PassportNumber,
            DateOfBirth = employee.DateOfBirth,
            GenderId = employee.GenderId,
            GenderNameEn = employee.Gender?.NameEn,
            GenderNameAr = employee.Gender?.NameAr,
            MaritalStatusId = employee.MaritalStatusId,
            MaritalStatusNameEn = employee.MaritalStatus?.NameEn,
            MaritalStatusNameAr = employee.MaritalStatus?.NameAr,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            MobileNumber = employee.MobileNumber,
            AddressAr = employee.AddressAr,
            AddressEn = employee.AddressEn,
            City = employee.City,
            Country = employee.Country,
            DepartmentId = employee.DepartmentId,
            DepartmentNameEn = employee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = employee.Department?.NameAr ?? string.Empty,
            JobTitleId = employee.JobTitleId,
            JobTitleEn = employee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = employee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = employee.DirectManagerId,
            DirectManagerName = employee.DirectManager?.FullNameEn,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.NameEn,
            ContractTypeId = employee.ContractTypeId,
            ContractTypeNameEn = employee.ContractType?.NameEn,
            ContractTypeNameAr = employee.ContractType?.NameAr,
            HiringDate = employee.HiringDate,
            ProbationPeriodMonths = employee.ProbationPeriodMonths,
            ProbationEndDate = employee.ProbationEndDate,
            StatusId = employee.StatusId,
            StatusNameEn = employee.Status?.NameEn,
            StatusNameAr = employee.Status?.NameAr,
            CreatedDate = employee.CreatedDate.DateTime
        };
    }

    public static async Task<string> GenerateEmployeeCodeAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var latestCode = await context.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeCode.StartsWith(EmployeeCodePrefix))
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => e.EmployeeCode)
            .FirstOrDefaultAsync(cancellationToken);

        var nextValue = 1;
        if (!string.IsNullOrWhiteSpace(latestCode))
        {
            var numericPart = latestCode[EmployeeCodePrefix.Length..];
            if (int.TryParse(numericPart, out var parsed))
            {
                nextValue = parsed + 1;
            }
        }

        string candidate;
        do
        {
            candidate = $"{EmployeeCodePrefix}{nextValue:D4}";
            nextValue++;
        }
        while (await context.Employees
            .AsNoTracking()
            .AnyAsync(e => e.EmployeeCode == candidate, cancellationToken));

        return candidate;
    }
}
