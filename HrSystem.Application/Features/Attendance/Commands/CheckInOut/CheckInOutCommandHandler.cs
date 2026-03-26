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

namespace HrSystem.Application.Features.Attendance.Commands.CheckInOut;

public class CheckInOutCommandHandler
    : IRequestHandler<CheckInOutCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public CheckInOutCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        CheckInOutCommand request, CancellationToken cancellationToken)
    {
        // ── 1) Resolve employee ──────────────────────────────
        Guid employeeId;
        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            employeeId = request.EmployeeId.Value;
        }
        else
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value == Guid.Empty)
                return Error.Unauthorized("Attendance.Unauthorized", "Current user is not linked to an employee.");

            employeeId = currentEmployeeId.Value;
        }

        // Authorization check
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr && CurrentUser.EmployeeId != employeeId)
            return Error.Unauthorized("Attendance.Unauthorized", "Not allowed to submit attendance for this employee.");

        // ── 2) Get employee and branch ───────────────────────
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted, cancellationToken);

        if (employee is null)
            return Error.NotFound("Employee.NotFound", "Employee not found.");

        var branchId = employee.BranchId;
        if (branchId == null || branchId == Guid.Empty)
            return Error.Validation("Employee.NoBranch", "Employee is not assigned to a branch.");

        // ── 3) Load branch attendance settings ───────────────
        var setting = await _context.BranchAttendanceSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == branchId && !s.IsDeleted, cancellationToken);

        // Validate method is allowed for this branch
        if (setting is not null)
        {
            var methodAllowed = request.Method switch
            {
                AttendanceMethod.FaceId => setting.AllowFaceId,
                AttendanceMethod.Location => setting.AllowLocation,
                AttendanceMethod.Fingerprint => setting.AllowFingerprint,
                AttendanceMethod.Manual => setting.AllowManual && isHr,
                AttendanceMethod.ExcelImport => false, // Excel import uses a different endpoint
                _ => false
            };

            if (!methodAllowed)
                return Error.Validation("Attendance.MethodNotAllowed",
                    $"Attendance method '{request.Method}' is not enabled for this branch.");
        }

        // ── 4) FaceId verification ───────────────────────────
        if (request.Method == AttendanceMethod.FaceId)
        {
            if (string.IsNullOrWhiteSpace(request.FaceTemplateBase64))
                return Error.Validation("Attendance.FaceIdRequired", "Face template is required for FaceId check-in.");

            var templateHash = ComputeTemplateHash(request.FaceTemplateBase64);
            var biometric = await _context.EmployeeBiometrics
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                    && b.BiometricType == BiometricType.FaceId
                    && b.IsActive, cancellationToken);

            if (biometric is null)
                return Error.NotFound("Biometric.NotFound", "No FaceId enrolled for this employee.");

            if (!string.Equals(biometric.TemplateHash, templateHash, StringComparison.OrdinalIgnoreCase))
                return Error.Unauthorized("Biometric.NotMatched", "FaceId verification failed.");
        }

        // ── 5) Location / geofence validation ────────────────
        Guid? matchedCheckInPointId = null;

        bool requireLocation = setting?.RequireLocationValidation == true
            && (request.Method == AttendanceMethod.Location
                || request.Method == AttendanceMethod.FaceId);

        if (requireLocation || request.Method == AttendanceMethod.Location)
        {
            if (!request.Latitude.HasValue || !request.Longitude.HasValue)
                return Error.Validation("Attendance.LocationRequired",
                    "GPS coordinates are required for location-based attendance.");

            // Load active check-in points for this branch
            // IgnoreQueryFilters bypasses the global tenant/branch scope filter,
            // which is too restrictive here — we already validated the employee's branch above.
            var checkInPoints = await _context.BranchCheckInPoints
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(p => p.BranchId == branchId && !p.IsDeleted && p.IsActive)
                .Where(p => request.PunchType == AttendancePunchType.CheckIn ? p.IsCheckInPoint : p.IsCheckOutPoint)
                .ToListAsync(cancellationToken);

            if (checkInPoints.Count == 0)
                return Error.Validation("Attendance.NoCheckInPoints",
                    "No check-in points configured for this branch. Please contact your administrator.");

            var defaultRadius = setting?.DefaultGeofenceRadiusMeters ?? 200;

            // Find the nearest check-in point within geofence
            BranchCheckInPointMatch? bestMatch = null;
            var nearestPointInfo = new List<string>();

            foreach (var point in checkInPoints)
            {
                var distance = CalculateDistanceMeters(
                    request.Latitude.Value, request.Longitude.Value,
                    point.Latitude, point.Longitude);

                var allowedRadius = point.RadiusMeters ?? defaultRadius;

                nearestPointInfo.Add($"{point.NameEn ?? point.NameAr ?? "Point"}: {distance:F2}m away (allowed: {allowedRadius}m)");

                if (distance <= allowedRadius)
                {
                    if (bestMatch is null || distance < bestMatch.Distance)
                    {
                        bestMatch = new BranchCheckInPointMatch(point.Id, distance);
                    }
                }
            }

            if (bestMatch is null)
            {
                var errorMessage = checkInPoints.Count == 1
                    ? $"You are not within the allowed check-in area. {nearestPointInfo[0]}"
                    : $"You are not within the allowed check-in area. Nearest points: {string.Join("; ", nearestPointInfo)}";

                return Error.Validation("Attendance.OutOfRange", errorMessage);
            }

            matchedCheckInPointId = bestMatch.PointId;
        }

        // ── 6) Create or update attendance record ────────────
        var eventTime = request.EventTime ?? DateTime.UtcNow;
        var date = eventTime.Date;
        var time = eventTime.TimeOfDay;

        var attendance = await _context.Attendances
            .IgnoreQueryFilters()
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a => !a.IsDeleted && !a.IsConfigurationRecord
                && a.EmployeeId == employeeId && a.Date == date, cancellationToken);

        if (attendance is null)
        {
            attendance = new AttendanceEntity
            {
                EmployeeId = employeeId,
                Date = date,
                StatusId = AttendanceStatusIds.Present,
                DeviceId = request.DeviceId,
                TenantId = employee.TenantId,
                BranchId = branchId
            };
            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
        }

        if (request.PunchType == AttendancePunchType.CheckIn)
        {
            // Check for duplicate check-in (unless multiple check-ins allowed)
            if (attendance.CheckInTime.HasValue && setting?.AllowMultipleCheckInsPerDay != true)
                return Error.Conflict("Attendance.AlreadyCheckedIn", "Employee already checked in today.");

            attendance.CheckInTime = time;
            attendance.CheckInDeviceId = request.DeviceId;
            attendance.CheckInLatitude = request.Latitude;
            attendance.CheckInLongitude = request.Longitude;
            attendance.CheckInPointId = matchedCheckInPointId;
            attendance.CheckInMethod = request.Method;
            attendance.DeviceId ??= request.DeviceId;
        }
        else
        {
            if (attendance.CheckOutTime.HasValue && setting?.AllowMultipleCheckInsPerDay != true)
                return Error.Conflict("Attendance.AlreadyCheckedOut", "Employee already checked out today.");

            // Validate minimum check-in duration
            if (attendance.CheckInTime.HasValue && setting?.MinCheckInDurationMinutes > 0)
            {
                var duration = time - attendance.CheckInTime.Value;
                if (duration.TotalMinutes < setting.MinCheckInDurationMinutes)
                    return Error.Validation("Attendance.TooShort",
                        $"Minimum time between check-in and check-out is {setting.MinCheckInDurationMinutes} minutes.");
            }

            attendance.CheckOutTime = time;
            attendance.CheckOutDeviceId = request.DeviceId;
            attendance.CheckOutLatitude = request.Latitude;
            attendance.CheckOutLongitude = request.Longitude;
            attendance.CheckOutPointId = matchedCheckInPointId;
            attendance.CheckOutMethod = request.Method;
            attendance.DeviceId ??= request.DeviceId;
        }

        // Recalculate worked hours
        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            attendance.Notes = string.IsNullOrEmpty(attendance.Notes)
                ? request.Notes
                : $"{attendance.Notes}; {request.Notes}";
        }

        attendance.StatusId = AttendanceStatusIds.Present;
        attendance.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        // Re-query for response DTO with navigations
        attendance = await _context.Attendances
            .IgnoreQueryFilters()
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == attendance.Id && !a.IsDeleted && !a.IsConfigurationRecord, cancellationToken);

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

        var action = request.PunchType == AttendancePunchType.CheckIn ? "Check-in" : "Check-out";
        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = $"{action} recorded successfully via {request.Method}.",
            Data = dto
        };
    }

    /// <summary>
    /// Haversine formula to calculate the distance in meters between two GPS coordinates.
    /// </summary>
    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusMeters = 6_371_000;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static string ComputeTemplateHash(string templateBase64)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(templateBase64.Trim());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private record BranchCheckInPointMatch(Guid PointId, double Distance);
}
