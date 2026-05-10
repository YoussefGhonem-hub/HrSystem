using ErrorOr;
using HrSystem.Application.Features.Employees.Commands;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Commands.UpdateProfileImage;

public record UpdateProfileImageCommand(Guid EmployeeId, IFormFile? ProfileImage) : IRequest<ErrorOr<GenericResponse<UpdateProfileImageResponse>>>;

public record UpdateProfileImageResponse(string? Url, string? Key);

public class UpdateProfileImageCommandHandler : IRequestHandler<UpdateProfileImageCommand, ErrorOr<GenericResponse<UpdateProfileImageResponse>>>
{
    private readonly HrSystem.Infrustructure.Persistence.ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateProfileImageCommandHandler(
        HrSystem.Infrustructure.Persistence.ApplicationDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<UpdateProfileImageResponse>>> Handle(
        UpdateProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Only HR/admin roles can update someone else's profile image
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            return Error.Forbidden(description: "Not authorized to update employee profile image");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == orgId.Value, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var editAccess = await EmployeeEditAuthorizationGuard.EnsureCanEditAsync(
            _context,
            employee.Id,
            employee.UserId,
            cancellationToken);
        if (editAccess.IsError)
        {
            return editAccess.Errors;
        }

        if (request.ProfileImage is null || request.ProfileImage.Length == 0)
        {
            return Error.Validation("ProfileImage.Required", "Profile image file is required");
        }

        var stored = await _storageService.Upload(request.ProfileImage, cancellationToken);
        if (stored == null || string.IsNullOrWhiteSpace(stored.Key))
        {
            return Error.Failure("ProfileImage.UploadFailed", "Failed to upload profile image");
        }

        var url = string.IsNullOrWhiteSpace(stored.Url) ? stored.Key : stored.Url;

        employee.ProfilePictureUrl = url;

        if (employee.UserId.HasValue)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == employee.UserId.Value, cancellationToken);
            if (user != null)
            {
                user.AvatarUrl = url;
                user.ModifiedDate = DateTimeOffset.UtcNow;
                user.ModifiedBy = CurrentUser.Id;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<UpdateProfileImageResponse>
        {
            Success = true,
            Message = "Profile image updated",
            Data = new UpdateProfileImageResponse(url, stored.Key)
        };
    }
}
