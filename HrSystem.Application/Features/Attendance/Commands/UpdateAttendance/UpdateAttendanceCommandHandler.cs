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
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsConfigurationRecord, cancellationToken);

        if (attendance == null)
        {
            return Error.NotFound(description: "Attendance record not found");
        }

        // Load branch work schedule so we can recalculate derived fields
        Domain.Entities.Organization.BranchWorkSchedule? schedule = null;
        if (attendance.Employee?.BranchId.HasValue == true)
        {
            schedule = await _context.BranchWorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BranchId == attendance.Employee.BranchId.Value
                    && s.IsActive && !s.IsDeleted, cancellationToken);
        }

        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.StatusId = request.StatusId;
        attendance.DeviceId = request.DeviceId;
        attendance.CheckInDeviceId = request.CheckInDeviceId;
        attendance.CheckOutDeviceId = request.CheckOutDeviceId;
        attendance.OvertimeHours = request.OvertimeHours;
        attendance.Notes = request.Notes;
        attendance.ApprovedBy = request.ApprovedBy;

        // Recalculate WorkedHours and derived punctuality fields from the updated times
        if (request.CheckInTime.HasValue && request.CheckOutTime.HasValue
            && request.CheckOutTime.Value >= request.CheckInTime.Value)
        {
            var worked = request.CheckOutTime.Value - request.CheckInTime.Value;
            attendance.WorkedHours = worked;

            if (schedule != null)
            {
                // Late minutes: how much after scheduled start (minus grace period)
                var checkInTod = request.CheckInTime.Value;
                var graceLate = schedule.GracePeriodLate ?? TimeSpan.Zero;
                var lateThreshold = schedule.StartTime + graceLate;
                if (checkInTod > lateThreshold)
                {
                    attendance.LateMinutes = checkInTod - schedule.StartTime;
                    attendance.IsLate = true;
                }
                else
                {
                    attendance.LateMinutes = TimeSpan.Zero;
                    attendance.IsLate = false;
                }

                // Early leave minutes: how much before scheduled end (minus grace period)
                var checkOutTod = request.CheckOutTime.Value;
                var graceEarly = schedule.GracePeriodEarlyLeave ?? TimeSpan.Zero;
                var earlyLeaveThreshold = schedule.EndTime - graceEarly;
                if (checkOutTod < earlyLeaveThreshold)
                {
                    attendance.EarlyLeaveMinutes = schedule.EndTime - checkOutTod;
                    attendance.IsEarlyLeave = true;
                }
                else
                {
                    attendance.EarlyLeaveMinutes = TimeSpan.Zero;
                    attendance.IsEarlyLeave = false;
                }

                // Overtime: if worked hours exceed the configured shift total
                var workedHours = (decimal)worked.TotalHours;
                attendance.IsOvertime = schedule.IsOvertimeEnabled && workedHours > schedule.OvertimeStartsAfterHours;
            }
            else
            {
                // No schedule available — honour caller-supplied values
                attendance.LateMinutes = request.LateMinutes;
                attendance.EarlyLeaveMinutes = request.EarlyLeaveMinutes;
                attendance.IsLate = request.IsLate;
                attendance.IsEarlyLeave = request.IsEarlyLeave;
                attendance.IsOvertime = request.IsOvertime;
            }
        }
        else
        {
            // Partial or no check-in/out — honour caller-supplied values
            attendance.LateMinutes = request.LateMinutes;
            attendance.EarlyLeaveMinutes = request.EarlyLeaveMinutes;
            attendance.IsLate = request.IsLate;
            attendance.IsEarlyLeave = request.IsEarlyLeave;
            attendance.IsOvertime = request.IsOvertime;
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
