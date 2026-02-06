using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Queries.GetAccountSettings;

public record GetAccountSettingsQuery(Guid UserId) : IRequest<ErrorOr<GenericResponse<AccountSettingsDto>>>;


public class GetAccountSettingsQueryHandler : IRequestHandler<GetAccountSettingsQuery, ErrorOr<GenericResponse<AccountSettingsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetAccountSettingsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AccountSettingsDto>>> Handle(
        GetAccountSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        var UserId = CurrentUser.UserId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }
        var emp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == request.UserId);
        var user = await _context.Users
            .Include(u => u.Employee)
            .Where(u => u.Id == emp.UserId)
            .Select(u => new AccountSettingsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                AccountStatus = u.IsActive ? "Active" : "Inactive",
                IsActive = u.IsActive,
                WorkEmail = u.Email,
                LastLogin = u.LastLogin,
                FullName = u.FullName,
                EmployeeId = u.EmployeeId,
                EmployeeName = u.Employee != null ? u.Employee.FullNameEn : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        return new GenericResponse<AccountSettingsDto>
        {
            Success = true,
            Message = "Account settings retrieved",
            Data = user
        };
    }
}
