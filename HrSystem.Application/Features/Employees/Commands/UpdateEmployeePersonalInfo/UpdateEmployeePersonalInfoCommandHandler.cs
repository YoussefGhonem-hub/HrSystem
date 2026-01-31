using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePersonalInfo;

public class UpdateEmployeePersonalInfoCommandHandler : IRequestHandler<UpdateEmployeePersonalInfoCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeePersonalInfoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeePersonalInfoCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        employee.FirstNameAr = request.FirstNameAr;
        employee.LastNameAr = request.LastNameAr;
        employee.FirstNameEn = request.FirstNameEn;
        employee.LastNameEn = request.LastNameEn;
        employee.NationalId = request.NationalId;
        employee.PassportNumber = request.PassportNumber;
        employee.DateOfBirth = request.DateOfBirth;
        employee.GenderId = request.GenderId;
        employee.MaritalStatusId = request.MaritalStatusId;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.MobileNumber = request.MobileNumber;
        employee.AddressAr = request.AddressAr;
        employee.AddressEn = request.AddressEn;
        employee.City = request.City;
        employee.Country = request.Country;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee personal info updated successfully",
            Data = dto
        };
    }
}
