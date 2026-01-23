using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        employee.FirstNameAr = request.FirstNameAr;
        employee.LastNameAr = request.LastNameAr;
        employee.FirstNameEn = request.FirstNameEn;
        employee.LastNameEn = request.LastNameEn;
        employee.PassportNumber = request.PassportNumber;
        employee.MaritalStatusId = request.MaritalStatusId;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.MobileNumber = request.MobileNumber;
        employee.AddressAr = request.AddressAr;
        employee.AddressEn = request.AddressEn;
        employee.City = request.City;
        employee.Country = request.Country;
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitleId = request.JobTitleId;
        employee.DirectManagerId = request.DirectManagerId;
        employee.BranchId = request.BranchId;
        employee.ContractTypeId = request.ContractTypeId;
        employee.StatusId = request.StatusId;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .Include(e => e.ContractType)
            .Include(e => e.Status)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        var dto = new EmployeeDto
        {
            Id = updatedEmployee.Id,
            EmployeeCode = updatedEmployee.EmployeeCode,
            FirstNameAr = updatedEmployee.FirstNameAr,
            LastNameAr = updatedEmployee.LastNameAr,
            FirstNameEn = updatedEmployee.FirstNameEn,
            LastNameEn = updatedEmployee.LastNameEn,
            FullNameAr = updatedEmployee.FullNameAr,
            FullNameEn = updatedEmployee.FullNameEn,
            NationalId = updatedEmployee.NationalId,
            PassportNumber = updatedEmployee.PassportNumber,
            DateOfBirth = updatedEmployee.DateOfBirth,
            GenderId = updatedEmployee.GenderId,
            GenderNameEn = updatedEmployee.Gender?.NameEn,
            GenderNameAr = updatedEmployee.Gender?.NameAr,
            MaritalStatusId = updatedEmployee.MaritalStatusId,
            MaritalStatusNameEn = updatedEmployee.MaritalStatus?.NameEn,
            MaritalStatusNameAr = updatedEmployee.MaritalStatus?.NameAr,
            Email = updatedEmployee.Email,
            PhoneNumber = updatedEmployee.PhoneNumber,
            MobileNumber = updatedEmployee.MobileNumber,
            AddressAr = updatedEmployee.AddressAr,
            AddressEn = updatedEmployee.AddressEn,
            City = updatedEmployee.City,
            Country = updatedEmployee.Country,
            DepartmentId = updatedEmployee.DepartmentId,
            DepartmentNameEn = updatedEmployee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = updatedEmployee.Department?.NameAr ?? string.Empty,
            JobTitleId = updatedEmployee.JobTitleId,
            JobTitleEn = updatedEmployee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = updatedEmployee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = updatedEmployee.DirectManagerId,
            DirectManagerName = updatedEmployee.DirectManager?.FullNameEn,
            BranchId = updatedEmployee.BranchId,
            BranchName = updatedEmployee.Branch?.NameEn,
            ContractTypeId = updatedEmployee.ContractTypeId,
            ContractTypeNameEn = updatedEmployee.ContractType?.NameEn,
            ContractTypeNameAr = updatedEmployee.ContractType?.NameAr,
            HiringDate = updatedEmployee.HiringDate,
            ProbationPeriodMonths = updatedEmployee.ProbationPeriodMonths,
            ProbationEndDate = updatedEmployee.ProbationEndDate,
            StatusId = updatedEmployee.StatusId,
            StatusNameEn = updatedEmployee.Status?.NameEn,
            StatusNameAr = updatedEmployee.Status?.NameAr,
            CreatedDate = updatedEmployee.CreatedDate.DateTime
        };

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee updated successfully",
            Data = dto
        };
    }
}
