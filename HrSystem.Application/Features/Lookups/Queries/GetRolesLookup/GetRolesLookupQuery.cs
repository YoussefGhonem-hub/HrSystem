using ErrorOr;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using HrSystem.Domain.Entities.Account;

namespace HrSystem.Application.Features.Lookups.Queries.GetRolesLookup;

public record GetRolesLookupQuery : IRequest<ErrorOr<GenericResponse<List<RoleLookupDto>>>>;

public class GetRolesLookupQueryHandler : IRequestHandler<GetRolesLookupQuery, ErrorOr<GenericResponse<List<RoleLookupDto>>>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public GetRolesLookupQueryHandler(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<List<RoleLookupDto>>>> Handle(
        GetRolesLookupQuery request,
        CancellationToken cancellationToken)
    {
        // Only return assignable roles (exclude SuperAdmin)
        var assignableRoles = new[]
        {
            RoleNames.OrganizationAdmin,
            RoleNames.HRManager,
            RoleNames.HRSpecialist,
            RoleNames.DepartmentManager,
            RoleNames.Employee
        };

        var roles = new List<RoleLookupDto>();
        foreach (var roleName in assignableRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roles.Add(new RoleLookupDto
                {
                    Id = role.Id,
                    NameEn = role.DisplayName ?? role.Name ?? roleName,
                    NameAr = role.DisplayName ?? role.Name ?? roleName,
                    Code = role.Name ?? roleName
                });
            }
        }

        return new GenericResponse<List<RoleLookupDto>>
        {
            Success = true,
            Message = "Roles retrieved successfully",
            Data = roles
        };
    }
}

public class RoleLookupDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
