using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.UpdateAttendance;

public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateAttendanceCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        UpdateAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsConfigurationRecord, cancellationToken);

        if (attendance == null)
        {
            return Error.NotFound(description: "Attendance record not found");
        }

        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.StatusId = request.StatusId;
        attendance.DeviceId = request.DeviceId;
        attendance.CheckInDeviceId = request.CheckInDeviceId;
        attendance.CheckOutDeviceId = request.CheckOutDeviceId;
        attendance.OvertimeHours = request.OvertimeHours;
        attendance.LateMinutes = request.LateMinutes;
        attendance.EarlyLeaveMinutes = request.EarlyLeaveMinutes;
        attendance.IsLate = request.IsLate;
        attendance.IsEarlyLeave = request.IsEarlyLeave;
        attendance.IsOvertime = request.IsOvertime;
        attendance.Notes = request.Notes;
        attendance.ApprovedBy = request.ApprovedBy;

        // Recalculate worked hours
        if (request.CheckInTime.HasValue && request.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = request.CheckOutTime.Value - request.CheckInTime.Value;
        }

        if (!string.IsNullOrEmpty(request.ApprovedBy))
        {
            attendance.ApprovedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedAttendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == attendance.Id && !a.IsConfigurationRecord, cancellationToken);

        var dto = new AttendanceDto
        {
            Id = updatedAttendance.Id,
            EmployeeId = updatedAttendance.EmployeeId,
            EmployeeName = updatedAttendance.Employee?.FullNameEn,
            EmployeeCode = updatedAttendance.Employee?.EmployeeCode,
            Date = updatedAttendance.Date,
            CheckInTime = updatedAttendance.CheckInTime,
            CheckOutTime = updatedAttendance.CheckOutTime,
            StatusId = updatedAttendance.StatusId,
            StatusNameEn = updatedAttendance.Status?.NameEn,
            StatusNameAr = updatedAttendance.Status?.NameAr,
            DeviceId = updatedAttendance.DeviceId,
            CheckInDeviceId = updatedAttendance.CheckInDeviceId,
            CheckOutDeviceId = updatedAttendance.CheckOutDeviceId,
            WorkedHours = updatedAttendance.WorkedHours,
            OvertimeHours = updatedAttendance.OvertimeHours,
            LateMinutes = updatedAttendance.LateMinutes,
            EarlyLeaveMinutes = updatedAttendance.EarlyLeaveMinutes,
            IsLate = updatedAttendance.IsLate,
            IsEarlyLeave = updatedAttendance.IsEarlyLeave,
            IsOvertime = updatedAttendance.IsOvertime,
            HalfDayRule = updatedAttendance.HalfDayRule,
            Notes = updatedAttendance.Notes,
            ApprovedBy = updatedAttendance.ApprovedBy,
            ApprovedDate = updatedAttendance.ApprovedDate
        };

        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance record updated successfully",
            Data = dto
        };
    }
}
