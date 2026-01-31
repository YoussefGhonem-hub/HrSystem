using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
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
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateEmployeeStatusAndProfileCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
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
}