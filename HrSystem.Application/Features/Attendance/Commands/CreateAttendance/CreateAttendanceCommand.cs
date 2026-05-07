using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.CreateAttendance;

public record CreateAttendanceCommand(
    Guid EmployeeId,
    DateTime Date,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    Guid StatusId,
    string? DeviceId,
    string? CheckInDeviceId,
    string? CheckOutDeviceId,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateAttendanceCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        CreateAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        // Calculate worked hours if both check-in and check-out are provided
        TimeSpan? workedHours = null;
        if (request.CheckInTime.HasValue && request.CheckOutTime.HasValue)
        {
            workedHours = request.CheckOutTime.Value - request.CheckInTime.Value;
        }

        var attendance = new Domain.Entities.Attendance.Attendance
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date.Date, // Store only date part
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            StatusId = request.StatusId,
            DeviceId = request.DeviceId,
            CheckInDeviceId = request.CheckInDeviceId,
            CheckOutDeviceId = request.CheckOutDeviceId,
            WorkedHours = workedHours,
            Notes = request.Notes,
            TenantId = Guid.Empty
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        var createdAttendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == attendance.Id, cancellationToken);

        var dto = new AttendanceDto
        {
            Id = createdAttendance.Id,
            EmployeeId = createdAttendance.EmployeeId,
            EmployeeName = createdAttendance.Employee?.FullNameEn,
            EmployeeCode = createdAttendance.Employee?.EmployeeCode,
            Date = createdAttendance.Date,
            CheckInTime = createdAttendance.CheckInTime,
            CheckOutTime = createdAttendance.CheckOutTime,
            StatusId = createdAttendance.StatusId,
            StatusNameEn = createdAttendance.Status?.NameEn,
            StatusNameAr = createdAttendance.Status?.NameAr,
            DeviceId = createdAttendance.DeviceId,
            CheckInDeviceId = createdAttendance.CheckInDeviceId,
            CheckOutDeviceId = createdAttendance.CheckOutDeviceId,
            WorkedHours = createdAttendance.WorkedHours,
            OvertimeHours = createdAttendance.OvertimeHours,
            LateMinutes = createdAttendance.LateMinutes,
            EarlyLeaveMinutes = createdAttendance.EarlyLeaveMinutes,
            IsLate = createdAttendance.IsLate,
            IsEarlyLeave = createdAttendance.IsEarlyLeave,
            IsOvertime = createdAttendance.IsOvertime,
            HalfDayRule = createdAttendance.HalfDayRule,
            Notes = createdAttendance.Notes,
            ApprovedBy = createdAttendance.ApprovedBy,
            ApprovedDate = createdAttendance.ApprovedDate
        };

        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance record created successfully",
            Data = dto
        };
    }
}
