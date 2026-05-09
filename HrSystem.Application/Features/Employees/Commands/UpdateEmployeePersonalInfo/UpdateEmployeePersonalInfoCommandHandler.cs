using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
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

        var isHrManager = CurrentUser.Roles.Any(r =>
            r.Equals(RoleNames.HRManager, StringComparison.OrdinalIgnoreCase));
        var isSuperAdmin = CurrentUser.IsSuperAdmin;

        if (isHrManager && CurrentUser.EmployeeId.HasValue && CurrentUser.EmployeeId.Value == employee.Id)
        {
            return Error.Forbidden("Employee.SelfEditForbidden", "HR Manager cannot edit their own profile.");
        }

        if (!isSuperAdmin && await IsAdminProfileAsync(employee.UserId, cancellationToken))
        {
            return Error.Forbidden("Employee.AdminProfileEditForbidden", "You are not allowed to edit admin profiles.");
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

    private async Task<bool> IsAdminProfileAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
            return false;

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(_context.Roles,
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.Any(name =>
            string.Equals(name, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }
}
