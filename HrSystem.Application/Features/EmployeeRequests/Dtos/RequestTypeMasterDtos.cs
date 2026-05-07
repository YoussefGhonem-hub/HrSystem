using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Dtos;

#region Base Request/Response DTOs
public record CreateRequestTypeMasterDto
{
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
    public bool RequireAttachment { get; init; }
}

public record UpdateRequestTypeMasterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
    public bool RequireAttachment { get; init; }
}
#endregion

#region RequestType DTOs (new master)
public record RequestTypeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public bool RequireAttachment { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region VacationType DTOs
public record CreateVacationTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPaid { get; init; } = true;
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
    public int? MaxDaysPerYear { get; init; }
}

public record UpdateVacationTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPaid { get; init; } = true;
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
    public int? MaxDaysPerYear { get; init; }
}

public record VacationTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPaid { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public int? MaxDaysPerYear { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region OvertimeType DTOs
public record CreateOvertimeTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultMultiplier { get; init; } = 1.5m;
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateOvertimeTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultMultiplier { get; init; } = 1.5m;
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record OvertimeTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultMultiplier { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region TrainingType DTOs
public record CreateTrainingTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateTrainingTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record TrainingTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region MiscellaneousType DTOs
public record CreateMiscellaneousTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateMiscellaneousTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record MiscellaneousTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region AttendanceCorrectionType DTOs
public record CreateAttendanceCorrectionTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateAttendanceCorrectionTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record AttendanceCorrectionTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region PersonalType DTOs
public record CreatePersonalTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdatePersonalTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record PersonalTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region FeedbackType DTOs
public record CreateFeedbackTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateFeedbackTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record FeedbackTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region PermissionType DTOs
public record CreatePermissionTypeDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdatePermissionTypeDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record PermissionTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool RequireAttachment { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region Branch Settings DTOs
public record BranchRequestSettingDetailDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string? BranchName { get; init; }
    public Guid RequestTypeId { get; init; }
    public string RequestTypeName { get; init; } = string.Empty;
    public bool IsVisibleToEmployees { get; init; }
    public bool AllowEmployeesToSubmit { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}

public record CreateBranchRequestSettingDto
{
    public Guid BranchId { get; init; }
    public Guid RequestTypeId { get; init; }
    public bool IsVisibleToEmployees { get; init; } = true;
    public bool AllowEmployeesToSubmit { get; init; } = true;
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
}

public record UpdateBranchRequestSettingDto
{
    public Guid Id { get; init; }
    public bool IsVisibleToEmployees { get; init; }
    public bool AllowEmployeesToSubmit { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
}

public record InitializeBranchSettingsDto
{
    public Guid BranchId { get; init; }
    public bool EnableAllRequestTypes { get; init; } = true;
}
#endregion
