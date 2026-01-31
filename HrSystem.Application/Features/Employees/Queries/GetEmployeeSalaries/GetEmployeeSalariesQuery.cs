using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;

public record GetEmployeeSalariesQuery(Guid EmployeeId)
    : IRequest<ErrorOr<GenericResponse<List<EmployeeSalaryDto>>>>;

public class GetEmployeeSalariesQueryHandler : IRequestHandler<GetEmployeeSalariesQuery, ErrorOr<GenericResponse<List<EmployeeSalaryDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeSalariesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeSalaryDto>>>> Handle(
        GetEmployeeSalariesQuery request,
        CancellationToken cancellationToken)
    {
        var employeeExists = await _context.Employees
            .AsNoTracking()
            .AnyAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (!employeeExists)
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
                return Error.Unauthorized("Salary.Unauthorized", "Not allowed to access employee salaries");
            }
        }

        var salaries = await _context.Salaries
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.EmployeeId == request.EmployeeId)
            .OrderByDescending(s => s.EffectiveDate)
            .Select(s => new EmployeeSalaryDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                BasicSalary = s.BasicSalary,
                EffectiveDate = s.EffectiveDate,
                EndDate = s.EndDate,
                Notes = s.Notes,
                IsCurrent = s.IsCurrent,
                Currency = s.Currency,
                IncludeSocialInsurance = s.IsSocialInsuranceEnabled,
                SocialInsuranceEmployeeRate = s.SocialInsuranceEmployeeRate,
                SocialInsuranceEmployerRate = s.SocialInsuranceEmployerRate,
                PaymentMethod = s.PaymentMethod
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<EmployeeSalaryDto>>
        {
            Success = true,
            Message = "Employee salaries retrieved successfully",
            Data = salaries
        };
    }
}
