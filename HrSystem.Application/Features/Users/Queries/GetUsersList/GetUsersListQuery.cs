using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Queries.GetUsersList;

public record GetUsersListQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    bool? IsActive = null,
    Guid? BranchId = null,
    Guid? RoleId = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<UserListItemDto>>>>;

public record UserListItemDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? FullName { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public List<UserBranchRoleDto> BranchRoles { get; init; } = new();
}

public record UserBranchRoleDto
{
    public Guid BranchId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}

public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, ErrorOr<GenericResponse<PagedResult<UserListItemDto>>>>
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public GetUsersListQueryHandler(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<UserListItemDto>>>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        string? roleNameFilter = null;
        if (request.RoleId.HasValue)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId.Value.ToString());
            if (role == null)
            {
                return Error.NotFound(code: "Role.NotFound", description: "Role not found");
            }
            roleNameFilter = role.Name;
        }

        var query = _context.Users
            .Where(u => u.OrganizationId == orgId.Value)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(u =>
                (u.Email ?? "").Contains(term) ||
                (u.UserName ?? "").Contains(term) ||
                (u.FullName ?? "").Contains(term));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(u => _context.UserBranchRoles.Any(ubr => ubr.UserId == u.Id && ubr.BranchId == request.BranchId.Value));
        }

        if (!string.IsNullOrEmpty(roleNameFilter))
        {
            query = query.Where(u => _context.UserBranchRoles.Any(ubr => ubr.UserId == u.Id && ubr.RoleName == roleNameFilter));
        }

        // Sorting
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "username" => request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "fullname" => request.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "createddate" => request.SortDescending ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate),
            _ => request.SortDescending ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate)
        };

        var projected = query.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName,
            FullName = u.FullName,
            IsActive = u.IsActive,
            CreatedDate = u.CreatedDate,
            BranchRoles = _context.UserBranchRoles
                .Where(ubr => ubr.UserId == u.Id)
                .Select(ubr => new UserBranchRoleDto
                {
                    BranchId = ubr.BranchId,
                    RoleName = ubr.RoleName
                }).ToList()
        });

        var paged = await projected.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new GenericResponse<PagedResult<UserListItemDto>>
        {
            Success = true,
            Message = "Users retrieved",
            Data = paged
        };
    }
}
