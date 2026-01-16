using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var probationEndDate = request.HiringDate.AddMonths(request.ProbationPeriodMonths);

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FirstNameAr = request.FirstNameAr,
            LastNameAr = request.LastNameAr,
            FirstNameEn = request.FirstNameEn,
            LastNameEn = request.LastNameEn,
            NationalId = request.NationalId,
            PassportNumber = request.PassportNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            MobileNumber = request.MobileNumber,
            AddressAr = request.AddressAr,
            AddressEn = request.AddressEn,
            City = request.City,
            Country = request.Country,
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            DirectManagerId = request.DirectManagerId,
            BranchId = request.BranchId,
            ContractType = request.ContractType,
            HiringDate = request.HiringDate,
            ProbationPeriodMonths = request.ProbationPeriodMonths,
            ProbationEndDate = probationEndDate,
            Status = EmployeeStatus.Active,
            TenantId = Guid.NewGuid() // Should come from CurrentUser.OrganizationId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        var dto = new EmployeeDto
        {
            Id = createdEmployee.Id,
            EmployeeCode = createdEmployee.EmployeeCode,
            FirstNameAr = createdEmployee.FirstNameAr,
            LastNameAr = createdEmployee.LastNameAr,
            FirstNameEn = createdEmployee.FirstNameEn,
            LastNameEn = createdEmployee.LastNameEn,
            FullNameAr = createdEmployee.FullNameAr,
            FullNameEn = createdEmployee.FullNameEn,
            NationalId = createdEmployee.NationalId,
            PassportNumber = createdEmployee.PassportNumber,
            DateOfBirth = createdEmployee.DateOfBirth,
            Gender = createdEmployee.Gender,
            MaritalStatus = createdEmployee.MaritalStatus,
            Email = createdEmployee.Email,
            PhoneNumber = createdEmployee.PhoneNumber,
            MobileNumber = createdEmployee.MobileNumber,
            AddressAr = createdEmployee.AddressAr,
            AddressEn = createdEmployee.AddressEn,
            City = createdEmployee.City,
            Country = createdEmployee.Country,
            DepartmentId = createdEmployee.DepartmentId,
            DepartmentNameEn = createdEmployee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = createdEmployee.Department?.NameAr ?? string.Empty,
            JobTitleId = createdEmployee.JobTitleId,
            JobTitleEn = createdEmployee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = createdEmployee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = createdEmployee.DirectManagerId,
            DirectManagerName = createdEmployee.DirectManager?.FullNameEn,
            BranchId = createdEmployee.BranchId,
            BranchName = createdEmployee.Branch?.NameEn,
            ContractType = createdEmployee.ContractType,
            HiringDate = createdEmployee.HiringDate,
            ProbationPeriodMonths = createdEmployee.ProbationPeriodMonths,
            ProbationEndDate = createdEmployee.ProbationEndDate,
            Status = createdEmployee.Status,
            CreatedDate = createdEmployee.CreatedDate.DateTime
        };

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee created successfully",
            Data = dto
        };
    }
}
