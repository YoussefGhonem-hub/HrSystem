using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeStatusAndProfile;

public record UpdateEmployeeStatusAndProfileCommand(
    Guid EmployeeId,
    Guid? StatusId,
    string? StatusReason,
    DateTime? StatusEffectiveDate,
    IFormFile? ProfileImage
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;

public class UpdateEmployeeStatusAndProfileCommandHandler : IRequestHandler<UpdateEmployeeStatusAndProfileCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private static readonly HashSet<Guid> AccessRevokingStatuses = new()
    {
        EmployeeStatusIds.Terminated,
        EmployeeStatusIds.Resigned
    };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateEmployeeStatusAndProfileCommandHandler(
        ApplicationDbContext context,
        IStorageService storageService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _storageService = storageService;
        _userManager = userManager;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeStatusAndProfileCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var isHrManager = CurrentUser.Roles.Any(r =>
            r.Equals(RoleNames.HRManager, StringComparison.OrdinalIgnoreCase));
        var isSuperAdmin = CurrentUser.IsSuperAdmin;

        if (isHrManager && CurrentUser.EmployeeId.HasValue && CurrentUser.EmployeeId.Value == employee.Id)
        {
            return Error.Forbidden("Employee.SelfEditForbidden", "HR Manager cannot edit their own profile.");
        }

        if (!isSuperAdmin && await IsAdminProfileAsync(employee.UserId, cancellationToken))
        {
            return Error.Forbidden("Employee.AdminProfileEditForbidden", "You are not allowed to edit admin profiles.");
        }

        if (request.StatusId.HasValue)
        {
            var statusExists = await _context.EmployeeStatuses
                .AnyAsync(s => s.Id == request.StatusId.Value, cancellationToken);

            if (!statusExists)
            {
                return Error.NotFound("EmployeeStatus.NotFound", "Employee status not found");
            }

            employee.StatusId = request.StatusId.Value;
            employee.TerminationReason = request.StatusReason;
            employee.TerminationDate = request.StatusEffectiveDate;

            // Revoke system access immediately upon termination or resignation.
            if (AccessRevokingStatuses.Contains(request.StatusId.Value) && employee.UserId.HasValue)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == employee.UserId.Value, cancellationToken);
                if (user != null)
                {
                    user.IsActive = false;
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    // Rotate security stamp so existing JWT tokens are rejected on next validation.
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }
        }

        if (request.ProfileImage is not null && request.ProfileImage.Length > 0)
        {
            var stored = await _storageService.Upload(request.ProfileImage, cancellationToken);
            if (stored == null || string.IsNullOrWhiteSpace(stored.Key))
            {
                return Error.Failure("Employee.ProfileImageUploadFailed", "Failed to upload profile image");
            }

            var fileUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);
            employee.ProfilePictureUrl = string.IsNullOrWhiteSpace(fileUrl) ? stored.Key : fileUrl;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee status/profile updated",
            Data = dto
        };
    }

    private async Task<bool> IsAdminProfileAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
            return false;

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(_context.Roles,
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.Any(name =>
            string.Equals(name, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }
}