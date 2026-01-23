using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using AttendanceEntity = HrSystem.Domain.Entities.Attendance.Attendance;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.VerifyBiometricAttendance;

public class VerifyBiometricAttendanceCommandHandler : IRequestHandler<VerifyBiometricAttendanceCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public VerifyBiometricAttendanceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        VerifyBiometricAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        Guid employeeId;
        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            employeeId = request.EmployeeId.Value;
        }
        else
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value == Guid.Empty)
            {
                return Error.Unauthorized("Attendance.Unauthorized", "Current user is not linked to an employee");
            }

            employeeId = currentEmployeeId.Value;
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr && CurrentUser.EmployeeId != employeeId)
        {
            return Error.Unauthorized("Attendance.Unauthorized", "Not allowed to submit attendance for this employee");
        }

        var templateHash = ComputeTemplateHash(request.TemplateBase64);

        var biometric = await _context.EmployeeBiometrics
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.BiometricType == request.BiometricType && b.IsActive, cancellationToken);

        if (biometric == null)
        {
            return Error.NotFound("Biometric.NotFound", "No biometric enrolled for this employee");
        }

        if (!string.Equals(biometric.TemplateHash, templateHash, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Unauthorized("Biometric.NotMatched", "Biometric verification failed");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var eventTime = request.EventTime ?? DateTime.UtcNow;
        var date = eventTime.Date;
        var time = eventTime.TimeOfDay;

        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a => !a.IsDeleted && a.EmployeeId == employeeId && a.Date == date, cancellationToken);

        if (attendance == null)
        {
            attendance = new AttendanceEntity
            {
                EmployeeId = employeeId,
                Date = date,
                StatusId = AttendanceStatusIds.Present,
                DeviceId = request.DeviceId,
                TenantId = employee.TenantId
            };

            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
        }

        if (request.PunchType == AttendancePunchType.CheckIn)
        {
            if (attendance.CheckInTime.HasValue)
            {
                return Error.Conflict("Attendance.AlreadyCheckedIn", "Employee already checked in");
            }

            attendance.CheckInTime = time;
            attendance.CheckInDeviceId = request.DeviceId;
            attendance.DeviceId ??= request.DeviceId;
        }
        else
        {
            if (attendance.CheckOutTime.HasValue)
            {
                return Error.Conflict("Attendance.AlreadyCheckedOut", "Employee already checked out");
            }

            attendance.CheckOutTime = time;
            attendance.CheckOutDeviceId = request.DeviceId;
            attendance.DeviceId ??= request.DeviceId;
        }

        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        }

        attendance.StatusId = AttendanceStatusIds.Present;
        attendance.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == attendance.Id, cancellationToken);

        var dto = new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = attendance.Employee?.FullNameEn,
            EmployeeCode = attendance.Employee?.EmployeeCode,
            Date = attendance.Date,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            StatusId = attendance.StatusId,
            StatusNameEn = attendance.Status?.NameEn,
            StatusNameAr = attendance.Status?.NameAr,
            DeviceId = attendance.DeviceId,
            CheckInDeviceId = attendance.CheckInDeviceId,
            CheckOutDeviceId = attendance.CheckOutDeviceId,
            WorkedHours = attendance.WorkedHours,
            OvertimeHours = attendance.OvertimeHours,
            LateMinutes = attendance.LateMinutes,
            EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
            IsLate = attendance.IsLate,
            IsEarlyLeave = attendance.IsEarlyLeave,
            IsOvertime = attendance.IsOvertime,
            Notes = attendance.Notes,
            ApprovedBy = attendance.ApprovedBy,
            ApprovedDate = attendance.ApprovedDate
        };

        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance recorded successfully",
            Data = dto
        };
    }

    private static string ComputeTemplateHash(string templateBase64)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(templateBase64.Trim());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
