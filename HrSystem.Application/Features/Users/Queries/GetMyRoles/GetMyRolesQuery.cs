using ErrorOr;
using HrSystem.Shared.Common;
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

        // Return the calling user's own assigned roles
        var currentRoleNames = CurrentUser.Roles.ToList();

        var roles = await _context.Roles
            .Where(r => currentRoleNames.Contains(r.Name!))
            .Select(r => new MyRoleDto(r.Id, r.Name ?? string.Empty, r.DisplayName))
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<MyRoleDto>>
        {
            Success = true,
            Message = "Roles retrieved",
            Data = roles
        };
    }
}
