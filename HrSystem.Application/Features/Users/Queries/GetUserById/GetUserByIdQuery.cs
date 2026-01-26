using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<UserDetailsDto>>>;

public record UserDetailsDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? FullName { get; init; }
    public bool IsActive { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid? EmployeeId { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
    public List<UserBranchRoleDto> BranchRoles { get; init; } = new();
}

public record UserBranchRoleDto
{
    public Guid BranchId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<GenericResponse<UserDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetUserByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<UserDetailsDto>>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var user = await _context.Users
            .Where(u => u.OrganizationId == orgId.Value && u.Id == request.Id)
            .Select(u => new UserDetailsDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName,
                FullName = u.FullName,
                IsActive = u.IsActive,
                AvatarUrl = u.AvatarUrl,
                OrganizationId = u.OrganizationId,
                EmployeeId = u.EmployeeId,
                CreatedDate = u.CreatedDate,
                ModifiedDate = u.ModifiedDate,
                BranchRoles = _context.UserBranchRoles
                    .Where(ubr => ubr.UserId == u.Id)
                    .Select(ubr => new UserBranchRoleDto
                    {
                        BranchId = ubr.BranchId,
                        RoleName = ubr.RoleName
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        return new GenericResponse<UserDetailsDto>
        {
            Success = true,
            Message = "User retrieved",
            Data = user
        };
    }
}
