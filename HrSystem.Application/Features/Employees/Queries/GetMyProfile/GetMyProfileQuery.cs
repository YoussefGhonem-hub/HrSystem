using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Queries.GetMyProfile;

/// <summary>
/// Query to get personal information for the currently logged-in user
/// </summary>
public record GetMyProfileQuery : IRequest<ErrorOr<GenericResponse<MyProfileDto>>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<GenericResponse<MyProfileDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyProfileQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyProfileDto>>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Employee.Unauthorized", "Current user is not linked to an employee");
        }

        var employee = await _context.Employees
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.Id == employeeId.Value, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var dto = new MyProfileDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullNameEn = employee.FullNameEn,
            FullNameAr = employee.FullNameAr,
            Nationality = employee.Country,
            GenderNameEn = employee.Gender?.NameEn,
            GenderNameAr = employee.Gender?.NameAr,
            DateOfBirth = employee.DateOfBirth,
            NationalIdNumber = employee.NationalId,
            MobileNumber = employee.MobileNumber,
            PersonalEmail = null,
            WorkEmail = employee.Email,
            JobTitleEn = employee.JobTitle?.TitleEn,
            JobTitleAr = employee.JobTitle?.TitleAr,
            DepartmentNameEn = employee.Department?.NameEn,
            DepartmentNameAr = employee.Department?.NameAr,
            EmploymentStatusEn = employee.Status?.NameEn,
            EmploymentStatusAr = employee.Status?.NameAr,
            HiringDate = employee.HiringDate,
            ProbationEndDate = employee.ProbationEndDate,
            MedicalInsuranceStatus = null
        };

        return new GenericResponse<MyProfileDto>
        {
            Success = true,
            Message = "Profile retrieved successfully",
            Data = dto
        };
    }
}
