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
        employee.MaritalStatus = request.MaritalStatus;
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
        employee.ContractType = request.ContractType;
        employee.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
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
            Gender = updatedEmployee.Gender,
            MaritalStatus = updatedEmployee.MaritalStatus,
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
            ContractType = updatedEmployee.ContractType,
            HiringDate = updatedEmployee.HiringDate,
            ProbationPeriodMonths = updatedEmployee.ProbationPeriodMonths,
            ProbationEndDate = updatedEmployee.ProbationEndDate,
            Status = updatedEmployee.Status,
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
