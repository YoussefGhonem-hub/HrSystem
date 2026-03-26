using ErrorOr;
using HrSystem.Application.Features.Attendance.Dtos;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.UpsertBranchAttendanceSetting;

public record UpsertBranchAttendanceSettingCommand(
    Guid BranchId,
    AttendanceMethod PrimaryMethod,
    bool AllowFaceId,
    bool AllowLocation,
    bool AllowExcelImport,
    bool AllowFingerprint,
    bool AllowManual,
    bool RequireLocationValidation,
    int DefaultGeofenceRadiusMeters,
    bool AutoCheckoutEnabled,
    TimeSpan? AutoCheckoutTime,
    double FaceIdConfidenceThreshold,
    bool FaceIdRequireLiveness,
    bool ExcelImportSkipDuplicates,
    string? ExcelDateFormat,
    bool AllowMultipleCheckInsPerDay,
    int MinCheckInDurationMinutes,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<BranchAttendanceSettingDto>>>;

public class UpsertBranchAttendanceSettingCommandHandler
    : IRequestHandler<UpsertBranchAttendanceSettingCommand, ErrorOr<GenericResponse<BranchAttendanceSettingDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpsertBranchAttendanceSettingCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchAttendanceSettingDto>>> Handle(
        UpsertBranchAttendanceSettingCommand request, CancellationToken cancellationToken)
    {
        // Validate branch exists
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && !b.IsDeleted, cancellationToken);

        if (branch is null)
            return Error.NotFound("Branch.NotFound", "Branch not found.");

        // Find existing setting or create new
        var setting = await _context.BranchAttendanceSettings
            .Include(s => s.CheckInPoints.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && !s.IsDeleted, cancellationToken);

        bool isNew = setting is null;

        if (isNew)
        {
            setting = new BranchAttendanceSetting
            {
                BranchId = request.BranchId
            };
        }

        // Map request to entity
        setting!.PrimaryMethod = request.PrimaryMethod;
        setting.AllowFaceId = request.AllowFaceId;
        setting.AllowLocation = request.AllowLocation;
        setting.AllowExcelImport = request.AllowExcelImport;
        setting.AllowFingerprint = request.AllowFingerprint;
        setting.AllowManual = request.AllowManual;
        setting.RequireLocationValidation = request.RequireLocationValidation;
        setting.DefaultGeofenceRadiusMeters = request.DefaultGeofenceRadiusMeters;
        setting.AutoCheckoutEnabled = request.AutoCheckoutEnabled;
        setting.AutoCheckoutTime = request.AutoCheckoutTime;
        setting.FaceIdConfidenceThreshold = request.FaceIdConfidenceThreshold;
        setting.FaceIdRequireLiveness = request.FaceIdRequireLiveness;
        setting.ExcelImportSkipDuplicates = request.ExcelImportSkipDuplicates;
        setting.ExcelDateFormat = request.ExcelDateFormat;
        setting.AllowMultipleCheckInsPerDay = request.AllowMultipleCheckInsPerDay;
        setting.MinCheckInDurationMinutes = request.MinCheckInDurationMinutes;
        setting.Notes = request.Notes;

        if (isNew)
            _context.BranchAttendanceSettings.Add(setting);

        await _context.SaveChangesAsync(cancellationToken);

        // Build response DTO
        var dto = new BranchAttendanceSettingDto
        {
            Id = setting.Id,
            BranchId = setting.BranchId,
            BranchNameEn = branch.NameEn,
            BranchNameAr = branch.NameAr,
            PrimaryMethod = setting.PrimaryMethod.ToString(),
            AllowFaceId = setting.AllowFaceId,
            AllowLocation = setting.AllowLocation,
            AllowExcelImport = setting.AllowExcelImport,
            AllowFingerprint = setting.AllowFingerprint,
            AllowManual = setting.AllowManual,
            RequireLocationValidation = setting.RequireLocationValidation,
            DefaultGeofenceRadiusMeters = setting.DefaultGeofenceRadiusMeters,
            AutoCheckoutEnabled = setting.AutoCheckoutEnabled,
            AutoCheckoutTime = setting.AutoCheckoutTime,
            FaceIdConfidenceThreshold = setting.FaceIdConfidenceThreshold,
            FaceIdRequireLiveness = setting.FaceIdRequireLiveness,
            ExcelImportSkipDuplicates = setting.ExcelImportSkipDuplicates,
            ExcelDateFormat = setting.ExcelDateFormat,
            AllowMultipleCheckInsPerDay = setting.AllowMultipleCheckInsPerDay,
            MinCheckInDurationMinutes = setting.MinCheckInDurationMinutes,
            Notes = setting.Notes,
            CheckInPoints = setting.CheckInPoints
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new CheckInPointDto
                {
                    Id = p.Id,
                    BranchId = p.BranchId ?? Guid.Empty,
                    NameAr = p.NameAr,
                    NameEn = p.NameEn,
                    Description = p.Description,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    RadiusMeters = p.RadiusMeters,
                    IsCheckInPoint = p.IsCheckInPoint,
                    IsCheckOutPoint = p.IsCheckOutPoint,
                    IsActive = p.IsActive,
                    Address = p.Address,
                    DisplayOrder = p.DisplayOrder
                }).ToList()
        };

        return new GenericResponse<BranchAttendanceSettingDto>
        {
            Success = true,
            Message = isNew
                ? "Branch attendance settings created successfully."
                : "Branch attendance settings updated successfully.",
            Data = dto
        };
    }
}
