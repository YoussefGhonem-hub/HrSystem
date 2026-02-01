using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Dtos;

#region Base Request/Response DTOs
public record CreateRequestTypeMasterDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}

public record UpdateRequestTypeMasterDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; } = 1;
}
#endregion

#region VacationType DTOs
public record CreateVacationTypeDto : CreateRequestTypeMasterDto
{
    public bool IsPaid { get; init; } = true;
    public int? MaxDaysPerYear { get; init; }
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdateVacationTypeDto : UpdateRequestTypeMasterDto
{
    public bool IsPaid { get; init; } = true;
    public int? MaxDaysPerYear { get; init; }
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record VacationTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPaid { get; init; }
    public int? MaxDaysPerYear { get; init; }
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region OvertimeType DTOs
public record CreateOvertimeTypeDto : CreateRequestTypeMasterDto
{
    public decimal DefaultMultiplier { get; init; } = 1.5m;
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdateOvertimeTypeDto : UpdateRequestTypeMasterDto
{
    public decimal DefaultMultiplier { get; init; } = 1.5m;
    public bool RequiresManagerApproval { get; init; } = true;
}

public record OvertimeTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultMultiplier { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region TrainingType DTOs
public record CreateTrainingTypeDto : CreateRequestTypeMasterDto
{
    public bool RequiresBudgetApproval { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdateTrainingTypeDto : UpdateRequestTypeMasterDto
{
    public bool RequiresBudgetApproval { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record TrainingTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresBudgetApproval { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region MiscellaneousType DTOs
public record CreateMiscellaneousTypeDto : CreateRequestTypeMasterDto
{
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdateMiscellaneousTypeDto : UpdateRequestTypeMasterDto
{
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record MiscellaneousTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region PersonalType DTOs
public record CreatePersonalTypeDto : CreateRequestTypeMasterDto
{
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdatePersonalTypeDto : UpdateRequestTypeMasterDto
{
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record PersonalTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresAttachment { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}
#endregion

#region FeedbackType DTOs
public record CreateFeedbackTypeDto : CreateRequestTypeMasterDto
{
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record UpdateFeedbackTypeDto : UpdateRequestTypeMasterDto
{
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; } = true;
}

public record FeedbackTypeDetailDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; }
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
    public EmployeeRequestType RequestType { get; init; }
    public string RequestTypeName { get; init; } = string.Empty;
    public bool IsVisibleToEmployees { get; init; }
    public bool AllowEmployeesToSubmit { get; init; }
    public bool RequireAttachment { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? ModifiedDate { get; init; }
}

public record CreateBranchRequestSettingDto
{
    public Guid BranchId { get; init; }
    public EmployeeRequestType RequestType { get; init; }
    public bool IsVisibleToEmployees { get; init; } = true;
    public bool AllowEmployeesToSubmit { get; init; } = true;
    public bool RequireAttachment { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
}

public record UpdateBranchRequestSettingDto
{
    public Guid Id { get; init; }
    public bool IsVisibleToEmployees { get; init; }
    public bool AllowEmployeesToSubmit { get; init; }
    public bool RequireAttachment { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }
}

public record InitializeBranchSettingsDto
{
    public Guid BranchId { get; init; }
    public bool EnableAllRequestTypes { get; init; } = true;
}
#endregion
