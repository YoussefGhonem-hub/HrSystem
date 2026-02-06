using ErrorOr;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Commands.UpdateMyProfileImage;

public record UpdateMyProfileImageCommand(IFormFile? ProfileImage) : IRequest<ErrorOr<GenericResponse<UpdateMyProfileImageResponse>>>;

public record UpdateMyProfileImageResponse(string? Url, string? Key);

public class UpdateMyProfileImageCommandHandler : IRequestHandler<UpdateMyProfileImageCommand, ErrorOr<GenericResponse<UpdateMyProfileImageResponse>>>
{
    private readonly HrSystem.Infrustructure.Persistence.ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateMyProfileImageCommandHandler(
        HrSystem.Infrustructure.Persistence.ApplicationDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<UpdateMyProfileImageResponse>>> Handle(
        UpdateMyProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUser.Id;
        var organizationId = CurrentUser.OrganizationId;

        if (!organizationId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        if (!userId.HasValue)
        {
            return Error.Unauthorized(description: "User context not found");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId.Value, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Employee linked to current user not found");
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

        return new GenericResponse<UpdateMyProfileImageResponse>
        {
            Success = true,
            Message = "Profile image updated",
            Data = new UpdateMyProfileImageResponse(url, stored.Key)
        };
    }
}
