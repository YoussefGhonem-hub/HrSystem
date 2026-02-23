using ErrorOr;
using HrSystem.Application.Features.Attendance.Dtos;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.CreateCheckInPoint;

public record CreateCheckInPointCommand(
    Guid BranchId,
    string NameAr,
    string NameEn,
    string? Description,
    double Latitude,
    double Longitude,
    int? RadiusMeters,
    bool IsCheckInPoint,
    bool IsCheckOutPoint,
    bool IsActive,
    string? Address,
    int DisplayOrder
) : IRequest<ErrorOr<GenericResponse<CheckInPointDto>>>;

public class CreateCheckInPointCommandHandler
    : IRequestHandler<CreateCheckInPointCommand, ErrorOr<GenericResponse<CheckInPointDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateCheckInPointCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<CheckInPointDto>>> Handle(
        CreateCheckInPointCommand request, CancellationToken cancellationToken)
    {
        // Validate branch exists
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && !b.IsDeleted, cancellationToken);

        if (branch is null)
            return Error.NotFound("Branch.NotFound", "Branch not found.");

        // Ensure attendance setting exists for this branch (auto-create if not)
        var setting = await _context.BranchAttendanceSettings
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && !s.IsDeleted, cancellationToken);

        if (setting is null)
        {
            setting = new BranchAttendanceSetting
            {
                BranchId = request.BranchId,
                AllowLocation = true
            };
            _context.BranchAttendanceSettings.Add(setting);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var point = new BranchCheckInPoint
        {
            BranchId = request.BranchId,
            BranchAttendanceSettingId = setting.Id,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusMeters = request.RadiusMeters,
            IsCheckInPoint = request.IsCheckInPoint,
            IsCheckOutPoint = request.IsCheckOutPoint,
            IsActive = request.IsActive,
            Address = request.Address,
            DisplayOrder = request.DisplayOrder
        };

        _context.BranchCheckInPoints.Add(point);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CheckInPointDto
        {
            Id = point.Id,
            BranchId = point.BranchId,
            NameAr = point.NameAr,
            NameEn = point.NameEn,
            Description = point.Description,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            RadiusMeters = point.RadiusMeters,
            IsCheckInPoint = point.IsCheckInPoint,
            IsCheckOutPoint = point.IsCheckOutPoint,
            IsActive = point.IsActive,
            Address = point.Address,
            DisplayOrder = point.DisplayOrder
        };

        return new GenericResponse<CheckInPointDto>
        {
            Success = true,
            Message = "Check-in point created successfully.",
            Data = dto
        };
    }
}
