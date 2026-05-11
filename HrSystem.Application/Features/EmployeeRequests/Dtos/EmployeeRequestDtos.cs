using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Dtos;

#region Master Type DTOs
public record VacationTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPaid { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record OvertimeTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultMultiplier { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record TrainingTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record MiscellaneousTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record PersonalTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record FeedbackTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsAnonymousAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record PermissionTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}

public record AttendanceCorrectionTypeDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public int SortOrder { get; init; }
}
#endregion

#region Branch Settings DTO
public record BranchRequestAvailabilityDto
{
    public Guid RequestTypeId { get; init; }
    public string RequestTypeCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string DisplayNameAr { get; init; } = string.Empty;
    public bool IsVisibleToEmployees { get; init; }
    public bool AllowEmployeesToSubmit { get; init; }
    public bool RequireAttachment { get; init; }
    public int? MaxOpenRequests { get; init; }
    public string? CustomInstructions { get; init; }

    // Type-specific options (only one is populated based on RequestType)
    public IReadOnlyCollection<VacationTypeDto>? VacationTypes { get; init; }
    public IReadOnlyCollection<TrainingTypeDto>? TrainingTypes { get; init; }
    public IReadOnlyCollection<MiscellaneousTypeDto>? MiscellaneousTypes { get; init; }
    public IReadOnlyCollection<PersonalTypeDto>? PersonalTypes { get; init; }
    public IReadOnlyCollection<FeedbackTypeDto>? FeedbackTypes { get; init; }
    public IReadOnlyCollection<PermissionTypeDto>? PermissionTypes { get; init; }
    public IReadOnlyCollection<AttendanceCorrectionTypeDto>? AttendanceCorrectionTypes { get; init; }
}
#endregion

#region Main Request DTO
public record EmployeeRequestDto
{
    public Guid Id { get; init; }
    public Guid RequestTypeId { get; init; }
    public string RequestTypeCode { get; init; } = string.Empty;
    public string RequestTypeName { get; init; } = string.Empty;
    public EmployeeRequestStatus Status { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? EmployeeCode { get; init; }
    public Guid? BranchId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime RequestedDate { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? ManagerComments { get; init; }
    public string? RejectionReason { get; init; }
    public Guid? ApprovedBy { get; init; }
    public string? ApprovedByName { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public Guid? ProcessedBy { get; init; }
    public string? ProcessedByName { get; init; }
    public DateTime? ProcessedDate { get; init; }

    /// <summary>
    /// Indicates who the request is currently pending at.
    /// "Manager", "HR", or null if not pending.
    /// </summary>
    public string? PendingAt { get; init; }

    // Type-specific details (only one is populated based on RequestType)
    public VacationDetailDto? VacationDetail { get; init; }
    public OvertimeDetailDto? OvertimeDetail { get; init; }
    public TrainingDetailDto? TrainingDetail { get; init; }
    public MiscellaneousDetailDto? MiscellaneousDetail { get; init; }
    public PersonalDetailDto? PersonalDetail { get; init; }
    public FeedbackDetailDto? FeedbackDetail { get; init; }
    public PermissionDetailDto? PermissionDetail { get; init; }
    public AttendanceCorrectionDetailDto? AttendanceCorrectionDetail { get; init; }
}
#endregion

#region Detail DTOs
public record VacationDetailDto
{
    public Guid VacationTypeId { get; init; }
    public string? VacationTypeName { get; init; }
    public decimal TotalDays { get; init; }
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public DateTime? ManagerApprovalDate { get; init; }
    public string? ManagerComments { get; init; }
    public Guid? HRApprovedBy { get; init; }
    public DateTime? HRApprovalDate { get; init; }
    public string? HRComments { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public record OvertimeDetailDto
{
    public Guid OvertimeTypeId { get; init; }
    public string? OvertimeTypeName { get; init; }
    public DateTime OvertimeDate { get; init; }
    public TimeSpan PlannedHours { get; init; }
    public TimeSpan? ActualHours { get; init; }
    public decimal Multiplier { get; init; }
    public string? ProjectCode { get; init; }
    public string? TaskDescription { get; init; }
    public Guid? ApprovedBy { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public string? ApprovalNotes { get; init; }

    /// <summary>
    /// Estimated overtime pay: (PlannedHours or ActualHours) × (BasicSalary / 240) × Multiplier
    /// </summary>
    public decimal? EstimatedOvertimeAmount { get; init; }

    /// <summary>
    /// Whether this overtime has been included in a generated payslip.
    /// </summary>
    public bool IsIncludedInPayslip { get; init; }

    /// <summary>
    /// The payslip month/year label if included (e.g. "April 2026").
    /// </summary>
    public string? PayslipPeriod { get; init; }
}

public record TrainingDetailDto
{
    public Guid TrainingTypeId { get; init; }
    public string? TrainingTypeName { get; init; }
    public string TrainingName { get; init; } = string.Empty;
    public string? TrainingProvider { get; init; }
    public string? TrainingLocation { get; init; }
    public DateTime TrainingStartDate { get; init; }
    public DateTime TrainingEndDate { get; init; }
    public int DurationDays { get; init; }
    public decimal? EstimatedCost { get; init; }
    public decimal? ApprovedBudget { get; init; }
    public string? Currency { get; init; }
    public string? Objectives { get; init; }
    public string? ExpectedOutcome { get; init; }
    public bool? CertificationObtained { get; init; }
    public string? CertificateUrl { get; init; }
}

public record MiscellaneousDetailDto
{
    public Guid MiscellaneousTypeId { get; init; }
    public string? MiscellaneousTypeName { get; init; }
    public string? AdditionalNotes { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Priority { get; init; }
    public DateTime? ExpectedCompletionDate { get; init; }
}

public record PersonalDetailDto
{
    public Guid PersonalTypeId { get; init; }
    public string? PersonalTypeName { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool IsUrgent { get; init; }
    public bool RequiresConfidentiality { get; init; }
    public string? PreferredContactMethod { get; init; }
    public string? AdditionalContactInfo { get; init; }
}

public record FeedbackDetailDto
{
    public Guid FeedbackTypeId { get; init; }
    public string? FeedbackTypeName { get; init; }
    public string FeedbackContent { get; init; } = string.Empty;
    public bool IsAnonymous { get; init; }
    public int? Rating { get; init; }
    public string? TargetDepartment { get; init; }
    public string? TargetPerson { get; init; }
    public string? SuggestedImprovement { get; init; }
    public bool ResponseRequired { get; init; }
    public string? ResponseContent { get; init; }
    public DateTime? ResponseDate { get; init; }
}

public record PermissionDetailDto
{
    public Guid PermissionTypeId { get; init; }
    public string? PermissionTypeName { get; init; }
    public DateTime PermissionDate { get; init; }
    public TimeSpan? FromTime { get; init; }
    public TimeSpan? ToTime { get; init; }
    public decimal TotalHours { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public DateTime? ManagerApprovalDate { get; init; }
    public string? ManagerComments { get; init; }
    public decimal? LeaveDeduction { get; init; }
}

public record AttendanceCorrectionDetailDto
{
    public Guid AttendanceCorrectionTypeId { get; init; }
    public string? AttendanceCorrectionTypeName { get; init; }
    public DateTime AttendanceDate { get; init; }
    public TimeSpan CorrectedTime { get; init; }
}
#endregion

