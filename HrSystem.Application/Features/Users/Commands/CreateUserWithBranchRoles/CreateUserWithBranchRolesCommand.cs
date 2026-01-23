using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.CreateUserWithBranchRoles;

public record CreateUserWithBranchRolesCommand(
    string Email,
    string FullName,
    string Password,
    string? UserName,
    Guid? OrganizationId,
    List<BranchRoleAssignment> BranchRoles,
    List<string>? GlobalRoles = null
) : IRequest<ErrorOr<GenericResponse<UserWithBranchRolesDto>>>;

public record BranchRoleAssignment(Guid BranchId, List<string> Roles);

public record UserWithBranchRolesDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
    public List<UserBranchRoleDto> BranchRoles { get; init; } = new();
}

public record UserBranchRoleDto
{
    public Guid BranchId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}

public class CreateUserWithBranchRolesCommandHandler : IRequestHandler<CreateUserWithBranchRolesCommand, ErrorOr<GenericResponse<UserWithBranchRolesDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateUserWithBranchRolesCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<UserWithBranchRolesDto>>> Handle(
        CreateUserWithBranchRolesCommand request,
        CancellationToken cancellationToken)
    {
        var organizationId = request.OrganizationId ?? CurrentUser.OrganizationId;
        if (!organizationId.HasValue)
        {
            return Error.Validation("Organization.Required", "OrganizationId is required");
        }

        var orgExists = await _context.Organizations
            .AnyAsync(o => o.Id == organizationId.Value, cancellationToken);

        if (!orgExists)
        {
            return Error.NotFound("Organization.NotFound", "Organization not found");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Error.Conflict("User.EmailExists", "User email already exists");
        }

        if (request.BranchRoles == null || request.BranchRoles.Count == 0)
        {
            return Error.Validation("User.BranchRolesRequired", "At least one branch role assignment is required");
        }

        var branchIds = request.BranchRoles.Select(b => b.BranchId).Distinct().ToList();
        var validBranchIds = await _context.Branches
            .Where(b => b.OrganizationId == organizationId.Value && branchIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        if (validBranchIds.Count != branchIds.Count)
        {
            return Error.Validation("Branch.Invalid", "One or more branches are invalid for this organization");
        }

        var allRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (request.GlobalRoles != null)
        {
            foreach (var role in request.GlobalRoles)
            {
                if (!RoleNames.IsValid(role))
                {
                    return Error.Validation("Role.Invalid", $"Invalid role: {role}");
                }

                allRoles.Add(role);
            }
        }

        foreach (var assignment in request.BranchRoles)
        {
            foreach (var role in assignment.Roles)
            {
                if (!RoleNames.IsValid(role))
                {
                    return Error.Validation("Role.Invalid", $"Invalid role: {role}");
                }

                allRoles.Add(role);
            }
        }

        foreach (var role in allRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return Error.NotFound("Role.NotFound", $"Role not found: {role}");
            }
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName,
            IsActive = true,
            OrganizationId = organizationId.Value,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Error.Validation("User.CreateFailed", string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        foreach (var role in allRoles)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignFailed", string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        var userBranchRoles = new List<UserBranchRole>();
        foreach (var assignment in request.BranchRoles)
        {
            foreach (var role in assignment.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                userBranchRoles.Add(new UserBranchRole
                {
                    UserId = user.Id,
                    BranchId = assignment.BranchId,
                    RoleName = role
                });
            }
        }

        await _context.UserBranchRoles.AddRangeAsync(userBranchRoles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        var dto = new UserWithBranchRolesDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            OrganizationId = organizationId.Value,
            BranchRoles = userBranchRoles.Select(x => new UserBranchRoleDto
            {
                BranchId = x.BranchId,
                RoleName = x.RoleName
            }).ToList()
        };

        return new GenericResponse<UserWithBranchRolesDto>
        {
            Success = true,
            Message = "User created successfully",
            Data = dto
        };
    }
}
