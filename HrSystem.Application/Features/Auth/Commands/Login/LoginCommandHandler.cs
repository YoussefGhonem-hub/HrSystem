using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Identity;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<GenericResponse<LoginResponse>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<GenericResponse<LoginResponse>>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by username
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Error.Unauthorized(description: "Invalid username or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Error.Forbidden(description: "User account is inactive");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Error.Unauthorized(description: "Invalid username or password");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Get employee information if user is linked to an employee
        Guid? employeeId = null;
        Guid? jobTitleId = null;
        Guid? directManagerId = null;
        string? departmentName = null;
        Guid? branchId = null;

        if (user.EmployeeId.HasValue)
        {
            var employee = await _context.Employees
                .Include(e => e.JobTitle)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value, cancellationToken);

            if (employee != null)
            {
                employeeId = employee.Id;
                jobTitleId = employee.JobTitleId;
                directManagerId = employee.DirectManagerId;
                departmentName = employee.Department?.NameEn ?? employee.Department?.NameAr;
                branchId = employee.BranchId;
            }
        }

        if (!branchId.HasValue && user.BranchId.HasValue)
        {
            branchId = user.BranchId;
        }

        // Fallback: resolve branch from user-branch role mapping if not set on employee
        if (!branchId.HasValue)
        {
            branchId = await _context.UserBranchRoles
                .Where(ubr => ubr.UserId == user.Id)
                .Select(ubr => (Guid?)ubr.BranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Generate JWT token with custom claims
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(
            user,
            roles,
            departmentName,
            employeeId,
            jobTitleId,
            directManagerId,
            branchId);

        var response = new LoginResponse(
            AccessToken: accessToken,
            ExpiresAt: expiresAt,
            UserId: user.Id.ToString(),
            FullName: user.FullName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Roles: roles.ToList());

        return new GenericResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = response
        };
    }
}
