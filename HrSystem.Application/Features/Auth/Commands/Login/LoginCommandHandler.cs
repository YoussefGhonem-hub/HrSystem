using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Identity;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            return Error.Unauthorized(description: "Invalid username or password");

        // Check if user is active
        if (!user.IsActive)
            return Error.Forbidden(description: "User account is inactive");

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Error.Unauthorized(description: "Invalid username or password");

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
            branchId = user.BranchId;

        // Fallback: resolve branch from user-branch role mapping if not set on employee
        if (!branchId.HasValue)
        {
            branchId = await _context.UserBranchRoles
                .Where(ubr => ubr.UserId == user.Id)
                .Select(ubr => (Guid?)ubr.BranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Best-effort update of last successful login timestamp
        var loginTime = DateTimeOffset.UtcNow;
        user.LastLogin = loginTime;
        await _userManager.UpdateAsync(user);

        var branchRequestAccess = new List<BranchRequestAvailabilityDto>();
        if (branchId.HasValue)
        {
            var branchSettings = await _context.BranchRequestSettings
                .AsNoTracking()
                .Include(s => s.RequestTypeRef)
                .Where(s => s.BranchId == branchId.Value && s.IsVisibleToEmployees)
                .OrderBy(s => s.RequestTypeRef != null ? s.RequestTypeRef.SortOrder : 0)
                .ToListAsync(cancellationToken);

            if (branchSettings.Count > 0)
            {
                var requestTypeCodes = branchSettings
                    .Where(s => s.RequestTypeRef != null)
                    .Select(s => s.RequestTypeRef!.Code)
                    .Distinct()
                    .ToList();

                // Load type-specific options
                var vacationTypes = requestTypeCodes.Contains("Vacation")
                    ? await _context.VacationTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new VacationTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsPaid = t.IsPaid, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var trainingTypes = requestTypeCodes.Contains("Training")
                    ? await _context.TrainingTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new TrainingTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var miscellaneousTypes = requestTypeCodes.Contains("Miscellaneous")
                    ? await _context.MiscellaneousTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new MiscellaneousTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var personalTypes = requestTypeCodes.Contains("Personal")
                    ? await _context.PersonalTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new PersonalTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var feedbackTypes = requestTypeCodes.Contains("Feedback")
                    ? await _context.FeedbackTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new FeedbackTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsAnonymousAllowed = t.IsAnonymousAllowed, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                branchRequestAccess = branchSettings
                    .Where(setting => setting.RequestTypeRef != null)
                    .Select(setting => new BranchRequestAvailabilityDto
                    {
                        RequestTypeId = setting.RequestTypeId,
                        RequestTypeCode = setting.RequestTypeRef!.Code,
                        DisplayName = setting.RequestTypeRef.NameEn,
                        DisplayNameAr = setting.RequestTypeRef.NameAr,
                        IsVisibleToEmployees = setting.IsVisibleToEmployees,
                        AllowEmployeesToSubmit = setting.AllowEmployeesToSubmit,
                        RequireAttachment = setting.RequireAttachment,
                        MaxOpenRequests = setting.MaxOpenRequests,
                        CustomInstructions = setting.CustomInstructions,
                        VacationTypes = setting.RequestTypeRef.Code == "Vacation" ? vacationTypes : null,
                        TrainingTypes = setting.RequestTypeRef.Code == "Training" ? trainingTypes : null,
                        MiscellaneousTypes = setting.RequestTypeRef.Code == "Miscellaneous" ? miscellaneousTypes : null,
                        PersonalTypes = setting.RequestTypeRef.Code == "Personal" ? personalTypes : null,
                        FeedbackTypes = setting.RequestTypeRef.Code == "Feedback" ? feedbackTypes : null
                    })
                    .ToList();
            }
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
            Roles: roles.ToList(),
            BranchId: branchId,
            EmployeeId: employeeId,
            BranchRequestAccess: branchRequestAccess);

        return new GenericResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = response
        };
    }
}
