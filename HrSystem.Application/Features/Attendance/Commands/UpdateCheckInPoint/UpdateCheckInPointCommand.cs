using ErrorOr;
using HrSystem.Application.Features.Attendance.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.UpdateCheckInPoint;

public record UpdateCheckInPointCommand(
    Guid Id,
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

public class UpdateCheckInPointCommandHandler
    : IRequestHandler<UpdateCheckInPointCommand, ErrorOr<GenericResponse<CheckInPointDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateCheckInPointCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<CheckInPointDto>>> Handle(
        UpdateCheckInPointCommand request, CancellationToken cancellationToken)
    {
        var point = await _context.BranchCheckInPoints
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (point is null)
            return Error.NotFound("CheckInPoint.NotFound", "Check-in point not found.");

        point.NameAr = request.NameAr;
        point.NameEn = request.NameEn;
        point.Description = request.Description;
        point.Latitude = request.Latitude;
        point.Longitude = request.Longitude;
        point.RadiusMeters = request.RadiusMeters;
        point.IsCheckInPoint = request.IsCheckInPoint;
        point.IsCheckOutPoint = request.IsCheckOutPoint;
        point.IsActive = request.IsActive;
        point.Address = request.Address;
        point.DisplayOrder = request.DisplayOrder;

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
            Message = "Check-in point updated successfully.",
            Data = dto
        };
    }
}
