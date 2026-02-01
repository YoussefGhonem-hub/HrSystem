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

        var branchRequestAccess = new List<BranchRequestAvailabilityDto>();
        if (branchId.HasValue)
        {
            var branchSettings = await _context.BranchRequestSettings
                .AsNoTracking()
                .Where(s => s.BranchId == branchId.Value && s.IsVisibleToEmployees)
                .OrderBy(s => s.RequestType)
                .ToListAsync(cancellationToken);

            if (branchSettings.Count > 0)
            {
                var requestTypes = branchSettings.Select(s => s.RequestType).Distinct().ToList();

                // Load type-specific options
                var vacationTypes = requestTypes.Contains(EmployeeRequestType.Vacation)
                    ? await _context.VacationTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new VacationTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsPaid = t.IsPaid, MaxDaysPerYear = t.MaxDaysPerYear, RequiresAttachment = t.RequiresAttachment,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var overtimeTypes = requestTypes.Contains(EmployeeRequestType.OverTime)
                    ? await _context.OvertimeTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new OvertimeTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            DefaultMultiplier = t.DefaultMultiplier, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var trainingTypes = requestTypes.Contains(EmployeeRequestType.Training)
                    ? await _context.TrainingTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new TrainingTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresBudgetApproval = t.RequiresBudgetApproval, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var miscellaneousTypes = requestTypes.Contains(EmployeeRequestType.Miscellaneous)
                    ? await _context.MiscellaneousTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new MiscellaneousTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresAttachment = t.RequiresAttachment, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var personalTypes = requestTypes.Contains(EmployeeRequestType.Personal)
                    ? await _context.PersonalTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new PersonalTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresAttachment = t.RequiresAttachment, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var feedbackTypes = requestTypes.Contains(EmployeeRequestType.Feedback)
                    ? await _context.FeedbackTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                        .Select(t => new FeedbackTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsAnonymousAllowed = t.IsAnonymousAllowed, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                branchRequestAccess = branchSettings
                    .Select(setting => new BranchRequestAvailabilityDto
                    {
                        RequestType = setting.RequestType,
                        DisplayName = setting.RequestType.ToString(),
                        IsVisibleToEmployees = setting.IsVisibleToEmployees,
                        AllowEmployeesToSubmit = setting.AllowEmployeesToSubmit,
                        RequireAttachment = setting.RequireAttachment,
                        MaxOpenRequests = setting.MaxOpenRequests,
                        CustomInstructions = setting.CustomInstructions,
                        VacationTypes = setting.RequestType == EmployeeRequestType.Vacation ? vacationTypes : null,
                        OvertimeTypes = setting.RequestType == EmployeeRequestType.OverTime ? overtimeTypes : null,
                        TrainingTypes = setting.RequestType == EmployeeRequestType.Training ? trainingTypes : null,
                        MiscellaneousTypes = setting.RequestType == EmployeeRequestType.Miscellaneous ? miscellaneousTypes : null,
                        PersonalTypes = setting.RequestType == EmployeeRequestType.Personal ? personalTypes : null,
                        FeedbackTypes = setting.RequestType == EmployeeRequestType.Feedback ? feedbackTypes : null
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
