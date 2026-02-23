using ErrorOr;
using HrSystem.Application.Features.Attendance.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetBranchAttendanceSetting;

public record GetBranchAttendanceSettingQuery(Guid BranchId)
    : IRequest<ErrorOr<GenericResponse<BranchAttendanceSettingDto>>>;

public class GetBranchAttendanceSettingQueryHandler
    : IRequestHandler<GetBranchAttendanceSettingQuery, ErrorOr<GenericResponse<BranchAttendanceSettingDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchAttendanceSettingQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchAttendanceSettingDto>>> Handle(
        GetBranchAttendanceSettingQuery request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && !b.IsDeleted, cancellationToken);

        if (branch is null)
            return Error.NotFound("Branch.NotFound", "Branch not found.");

        var setting = await _context.BranchAttendanceSettings
            .AsNoTracking()
            .Include(s => s.CheckInPoints.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && !s.IsDeleted, cancellationToken);

        if (setting is null)
        {
            // Return default settings (not yet configured)
            return new GenericResponse<BranchAttendanceSettingDto>
            {
                Success = true,
                Message = "No attendance settings configured for this branch. Showing defaults.",
                Data = new BranchAttendanceSettingDto
                {
                    BranchId = branch.Id,
                    BranchNameEn = branch.NameEn,
                    BranchNameAr = branch.NameAr,
                    PrimaryMethod = "Manual",
                    AllowManual = true,
                    DefaultGeofenceRadiusMeters = 200,
                    FaceIdConfidenceThreshold = 0.85,
                    FaceIdRequireLiveness = true,
                    ExcelImportSkipDuplicates = true,
                    MinCheckInDurationMinutes = 1
                }
            };
        }

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
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new CheckInPointDto
                {
                    Id = p.Id,
                    BranchId = p.BranchId,
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
            Data = dto
        };
    }
}
