using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Audit;

/// <summary>
/// Immutable audit trail entry. One row is written for every create / update / delete /
/// restore of an audited entity (employees, attendance records and organization settings),
/// capturing who did it, when, from where, and the exact field-level changes.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Functional area: Employees, Attendance, OrganizationSettings.</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>CLR entity type name, e.g. Employee, Attendance, BranchWorkSchedule.</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Primary key of the audited record.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Human readable label for the audited record (e.g. "EMP-1051 - Doha Ahmed").</summary>
    public string? EntityDisplay { get; set; }

    /// <summary>Created, Updated, Deleted or Restored.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>JSON array of { field, oldValue, newValue } describing the change set.</summary>
    public string? ChangesJson { get; set; }

    /// <summary>Number of fields that changed (denormalised for list views).</summary>
    public int ChangedFieldsCount { get; set; }

    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public static class AuditActions
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
    public const string Restored = "Restored";
}

public static class AuditModules
{
    public const string Employees = "Employees";
    public const string Attendance = "Attendance";
    public const string OrganizationSettings = "OrganizationSettings";
}
