using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Identity;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HrSystem.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<GenericResponse<LoginResponse>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<GenericResponse<LoginResponse>>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by username
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Error.Unauthorized(description: "Invalid username or password");

        // Check if user is active
        if (!user.IsActive)
            return Error.Forbidden(description: "User account is inactive");

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Error.Unauthorized(description: "Invalid username or password");

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Get employee information if user is linked to an employee
        Guid? employeeId = null;
        Guid? jobTitleId = null;
        Guid? directManagerId = null;
        string? departmentName = null;
        Guid? branchId = null;

        if (user.EmployeeId.HasValue)
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .Include(e => e.JobTitle)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value && !e.IsDeleted, cancellationToken);

            if (employee != null)
            {
                employeeId = employee.Id;
                jobTitleId = employee.JobTitleId;
                directManagerId = employee.DirectManagerId;
                departmentName = employee.Department?.NameEn ?? employee.Department?.NameAr;
                branchId = employee.BranchId;
            }
        }

        if (!branchId.HasValue && user.BranchId.HasValue)
            branchId = user.BranchId;

        // Fallback: resolve branch from user-branch role mapping if not set on employee
        if (!branchId.HasValue)
        {
            branchId = await _context.UserBranchRoles
                .IgnoreQueryFilters()
                .Where(ubr => ubr.UserId == user.Id)
                .Select(ubr => (Guid?)ubr.BranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Best-effort update of last successful login timestamp
        var loginTime = DateTimeOffset.UtcNow;
        user.LastLogin = loginTime;
        await _userManager.UpdateAsync(user);

        var branchRequestAccess = new List<BranchRequestAvailabilityDto>();
        if (branchId.HasValue)
        {
            var branchSettings = await _context.BranchRequestSettings
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.RequestTypeRef)
                .Where(s => s.BranchId == branchId.Value && !s.IsDeleted && s.IsVisibleToEmployees)
                .OrderBy(s => s.RequestTypeRef != null ? s.RequestTypeRef.SortOrder : 0)
                .ToListAsync(cancellationToken);

            if (branchSettings.Count > 0)
            {
                var requestTypeCodes = branchSettings
                    .Where(s => s.RequestTypeRef != null)
                    .Select(s => s.RequestTypeRef!.Code)
                    .Distinct()
                    .ToList();

                // Load type-specific options
                var vacationTypes = requestTypeCodes.Contains("Vacation")
                    ? await _context.VacationTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new VacationTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsPaid = t.IsPaid, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var trainingTypes = requestTypeCodes.Contains("Training")
                    ? await _context.TrainingTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new TrainingTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var miscellaneousTypes = requestTypeCodes.Contains("Miscellaneous")
                    ? await _context.MiscellaneousTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new MiscellaneousTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var personalTypes = requestTypeCodes.Contains("Personal")
                    ? await _context.PersonalTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new PersonalTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var feedbackTypes = requestTypeCodes.Contains("Feedback")
                    ? await _context.FeedbackTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new FeedbackTypeDto
                        {
                            Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                            IsAnonymousAllowed = t.IsAnonymousAllowed, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                var attendanceCorrectionTypes = requestTypeCodes.Contains("AttendanceCorrection")
                    ? await _context.AttendanceCorrectionTypes.IgnoreQueryFilters().AsNoTracking().Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.SortOrder)
                        .Select(t => new AttendanceCorrectionTypeDto
                        {
                            Id = t.Id,
                            NameEn = t.NameEn,
                            NameAr = t.NameAr,
                            Description = t.Description,
                            RequiresManagerApproval = t.RequiresManagerApproval,
                            SortOrder = t.SortOrder
                        }).ToListAsync(cancellationToken)
                    : null;

                branchRequestAccess = branchSettings
                    .Where(setting => setting.RequestTypeRef != null)
                    .Select(setting => new BranchRequestAvailabilityDto
                    {
                        RequestTypeId = setting.RequestTypeId,
                        RequestTypeCode = setting.RequestTypeRef!.Code,
                        DisplayName = setting.RequestTypeRef.NameEn,
                        DisplayNameAr = setting.RequestTypeRef.NameAr,
                        IsVisibleToEmployees = setting.IsVisibleToEmployees,
                        AllowEmployeesToSubmit = setting.AllowEmployeesToSubmit,
                        RequireAttachment = setting.RequestTypeRef?.RequireAttachment ?? false,
                        MaxOpenRequests = setting.MaxOpenRequests,
                        CustomInstructions = setting.CustomInstructions,
                        VacationTypes = setting.RequestTypeRef.Code == "Vacation" ? vacationTypes : null,
                        TrainingTypes = setting.RequestTypeRef.Code == "Training" ? trainingTypes : null,
                        MiscellaneousTypes = setting.RequestTypeRef.Code == "Miscellaneous" ? miscellaneousTypes : null,
                        PersonalTypes = setting.RequestTypeRef.Code == "Personal" ? personalTypes : null,
                        FeedbackTypes = setting.RequestTypeRef.Code == "Feedback" ? feedbackTypes : null,
                        AttendanceCorrectionTypes = setting.RequestTypeRef.Code == "AttendanceCorrection" ? attendanceCorrectionTypes : null
                    })
                    .ToList();
            }
        }

        // Generate JWT token with custom claims
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(
            user,
            roles,
            departmentName,
            employeeId,
            jobTitleId,
            directManagerId,
            branchId);

        // ── Load organization settings ────────────────────────
        OrganizationSettingDto? organizationSetting = null;
        var isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin && user.OrganizationId != Guid.Empty)
        {
            var org = await _context.Organizations
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == user.OrganizationId && !o.IsDeleted, cancellationToken);

            if (org is not null)
            {
                organizationSetting = new OrganizationSettingDto
                {
                    OrganizationId = org.Id,
                    NameAr = org.NameAr,
                    NameEn = org.NameEn,
                    Code = org.Code,
                    Industry = org.Industry,
                    LogoUrl = org.LogoUrl,
                    Email = org.Email,
                    PhoneNumber = org.PhoneNumber,
                    Website = org.Website,
                    AddressAr = org.AddressAr,
                    AddressEn = org.AddressEn,
                    City = org.City,
                    Country = org.Country,
                    TimeZone = org.TimeZone,
                    Currency = org.Currency,
                    WeekStartDay = org.WeekStartDay,
                    DefaultLanguage = LanguageDefaults.NormalizeOrDefault(org.DefaultLanguage),
                    IsActive = org.IsActive,
                    IsTrialPeriod = org.IsTrialPeriod,
                    TrialEndDate = org.TrialEndDate,
                    SubscriptionEndDate = org.SubscriptionEndDate,
                    MaxEmployees = org.MaxEmployees,
                    CurrentEmployeeCount = org.CurrentEmployeeCount
                };
            }
        }

        // ── Load branch attendance configuration ─────────────
        BranchAttendanceConfigDto? branchAttendanceConfig = null;
        if (branchId.HasValue)
        {
            var attendanceSetting = await _context.BranchAttendanceSettings
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.CheckInPoints.Where(p => !p.IsDeleted && p.IsActive))
                .FirstOrDefaultAsync(s => s.BranchId == branchId.Value && !s.IsDeleted, cancellationToken);

            if (attendanceSetting is not null)
            {
                // ── Check biometric enrollment status ────────────
                BiometricEnrollmentDto? biometricEnrollment = null;
                if (employeeId.HasValue)
                {
                    var enrolledBiometrics = await _context.EmployeeBiometrics
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(b => b.EmployeeId == employeeId.Value && b.IsActive && !b.IsDeleted)
                        .Select(b => b.BiometricType)
                        .ToListAsync(cancellationToken);

                    var hasFaceId = enrolledBiometrics.Contains(Domain.Enums.BiometricType.FaceId);
                    var hasFingerprint = enrolledBiometrics.Contains(Domain.Enums.BiometricType.Fingerprint);

                    var pendingMethods = new List<string>();
                    if (attendanceSetting.AllowFaceId && !hasFaceId)
                        pendingMethods.Add("FaceId");
                    if (attendanceSetting.AllowFingerprint && !hasFingerprint)
                        pendingMethods.Add("Fingerprint");

                    biometricEnrollment = new BiometricEnrollmentDto
                    {
                        IsFaceIdEnrolled = hasFaceId,
                        IsFingerprintEnrolled = hasFingerprint,
                        RequiresEnrollment = pendingMethods.Count > 0,
                        PendingEnrollmentMethods = pendingMethods
                    };
                }

                branchAttendanceConfig = new BranchAttendanceConfigDto
                {
                    SettingId = attendanceSetting.Id,
                    BranchId = attendanceSetting.BranchId,
                    PrimaryMethod = attendanceSetting.PrimaryMethod.ToString(),
                    AllowFaceId = attendanceSetting.AllowFaceId,
                    AllowLocation = attendanceSetting.AllowLocation,
                    AllowFingerprint = attendanceSetting.AllowFingerprint,
                    AllowManual = attendanceSetting.AllowManual,
                    RequireLocationValidation = attendanceSetting.RequireLocationValidation,
                    DefaultGeofenceRadiusMeters = attendanceSetting.DefaultGeofenceRadiusMeters,
                    FaceIdConfidenceThreshold = attendanceSetting.FaceIdConfidenceThreshold,
                    FaceIdRequireLiveness = attendanceSetting.FaceIdRequireLiveness,
                    AllowMultipleCheckInsPerDay = attendanceSetting.AllowMultipleCheckInsPerDay,
                    CheckInPoints = attendanceSetting.CheckInPoints
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => new CheckInPointInfoDto
                        {
                            Id = p.Id,
                            NameAr = p.NameAr,
                            NameEn = p.NameEn,
                            Latitude = p.Latitude,
                            Longitude = p.Longitude,
                            RadiusMeters = p.RadiusMeters,
                            IsCheckInPoint = p.IsCheckInPoint,
                            IsCheckOutPoint = p.IsCheckOutPoint,
                            Address = p.Address
                        }).ToList(),
                    BiometricEnrollment = biometricEnrollment
                };
            }
        }

        var response = new LoginResponse(
            AccessToken: accessToken,
            ExpiresAt: expiresAt,
            UserId: user.Id.ToString(),
            FullName: user.FullName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Roles: roles.ToList(),
            BranchId: branchId,
            EmployeeId: employeeId,
            BranchRequestAccess: branchRequestAccess,
            BranchAttendanceConfig: branchAttendanceConfig,
            OrganizationSetting: organizationSetting);

        return new GenericResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = response
        };
    }
}
