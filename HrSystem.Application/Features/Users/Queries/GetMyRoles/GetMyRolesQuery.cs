using ErrorOr;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Queries.GetMyRoles;

public record MyRoleDto(Guid Id, string Name, string? DisplayName);

public record GetMyRolesQuery() : IRequest<ErrorOr<GenericResponse<List<MyRoleDto>>>>;

public class GetMyRolesQueryHandler : IRequestHandler<GetMyRolesQuery, ErrorOr<GenericResponse<List<MyRoleDto>>>>
{
    private readonly HrSystem.Infrustructure.Persistence.ApplicationDbContext _context;

    public GetMyRolesQueryHandler(HrSystem.Infrustructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<MyRoleDto>>>> Handle(GetMyRolesQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUser.Id;
        if (!userId.HasValue)
        {
            return Error.Unauthorized(description: "User context not found");
        }

        var roles = await _context.Roles
            .Select(r => new MyRoleDto(r.Id, r.Name ?? string.Empty, r.DisplayName))
            .ToListAsync(cancellationToken);

        roles = ApplyVisibilityFilter(roles);

        return new GenericResponse<List<MyRoleDto>>
        {
            Success = true,
            Message = "Roles retrieved",
            Data = roles
        };
    }

    private static List<MyRoleDto> ApplyVisibilityFilter(List<MyRoleDto> roles)
    {
        var currentRoles = CurrentUser.Roles ?? Array.Empty<string>();

        bool isSuperAdmin = currentRoles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase);
        bool isOrgAdmin = currentRoles.Contains(RoleNames.OrganizationAdmin, StringComparer.OrdinalIgnoreCase);
        bool isHrManager = currentRoles.Contains(RoleNames.HRManager, StringComparer.OrdinalIgnoreCase);
        bool isHrSpecialist = currentRoles.Contains(RoleNames.HRSpecialist, StringComparer.OrdinalIgnoreCase);
        bool isHr = isHrManager || isHrSpecialist;

        // If caller is OrgAdmin or HR (and not SuperAdmin), restrict to HR + Employee roles only
        if (!isSuperAdmin && (isOrgAdmin || isHr))
        {
            roles = roles
                .Where(r => r.Name.Equals(RoleNames.HRManager, StringComparison.OrdinalIgnoreCase)
                            || r.Name.Equals(RoleNames.HRSpecialist, StringComparison.OrdinalIgnoreCase)
                            || r.Name.Equals(RoleNames.Employee, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return roles;
    }
}
