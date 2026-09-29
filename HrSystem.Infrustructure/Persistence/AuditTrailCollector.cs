using System.Globalization;
using System.Text.Json;
using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Audit;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HrSystem.Infrustructure.Persistence;

/// <summary>
/// Inspects the change tracker right before SaveChanges and writes an <see cref="AuditLog"/>
/// row for every create / update / delete / restore of an audited entity.
///
/// Audited modules:
///   - Employees:             Employee, EmployeeDocument
///   - Attendance:            Attendance, EmployeeBiometric
///   - OrganizationSettings:  Organization, OrganizationPayrollSettings, Branch,
///                            BranchWorkSchedule, BranchHoliday, BranchAttendanceSetting,
///                            BranchCheckInPoint, BranchRequestSetting, Department, JobTitle
/// </summary>
public static class AuditTrailCollector
{
    private const int MaxValueLength = 500;

    private static readonly Dictionary<Type, string> AuditedTypes = new()
    {
        [typeof(Employee)] = AuditModules.Employees,
        [typeof(EmployeeDocument)] = AuditModules.Employees,

        [typeof(Attendance)] = AuditModules.Attendance,
        [typeof(EmployeeBiometric)] = AuditModules.Attendance,

        [typeof(Organization)] = AuditModules.OrganizationSettings,
        [typeof(OrganizationPayrollSettings)] = AuditModules.OrganizationSettings,
        [typeof(Branch)] = AuditModules.OrganizationSettings,
        [typeof(BranchWorkSchedule)] = AuditModules.OrganizationSettings,
        [typeof(BranchHoliday)] = AuditModules.OrganizationSettings,
        [typeof(BranchAttendanceSetting)] = AuditModules.OrganizationSettings,
        [typeof(BranchCheckInPoint)] = AuditModules.OrganizationSettings,
        [typeof(BranchRequestSetting)] = AuditModules.OrganizationSettings,
        [typeof(Department)] = AuditModules.OrganizationSettings,
        [typeof(JobTitle)] = AuditModules.OrganizationSettings,
    };

    /// <summary>Bookkeeping columns that are never reported as user-visible changes.</summary>
    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(BaseEntity.Id),
        nameof(BaseEntity.TenantId),
        nameof(BaseEntity.CreatedDate),
        nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.ModifiedBy),
        nameof(BaseEntity.ModifiedDate),
        nameof(BaseEntity.IsDeleted),
        nameof(BaseEntity.DeletedDate),
        nameof(BaseEntity.DeletedBy),
        "RowVersion",
        "ConcurrencyStamp"
    };

    /// <summary>Sensitive / bulky columns whose values must never be written to the trail.</summary>
    private static readonly string[] RedactedPropertyFragments =
    {
        "Password", "Hash", "Template", "Base64", "Secret", "Token"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static bool IsAudited(Type clrType) => AuditedTypes.ContainsKey(clrType);

    /// <summary>
    /// Must be called AFTER the soft-delete conversion (Deleted -> Modified + IsDeleted) and
    /// BEFORE the base SaveChanges so the audit rows are persisted in the same transaction.
    /// </summary>
    public static void Collect(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && AuditedTypes.ContainsKey(e.Metadata.ClrType))
            .ToList();

        if (entries.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        var userId = CurrentUser.Id;
        var userName = SafeUserName();
        var ip = SafeIpAddress();

        var logs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var log = BuildLog(context, entry, now, userId, userName, ip);
            if (log != null)
                logs.Add(log);
        }

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    private static AuditLog? BuildLog(
        DbContext context,
        EntityEntry<BaseEntity> entry,
        DateTimeOffset now,
        Guid? userId,
        string? userName,
        string? ip)
    {
        var entity = entry.Entity;
        var clrType = entry.Metadata.ClrType;
        var module = AuditedTypes[clrType];

        string action;
        List<AuditFieldChange> changes;

        if (entry.State == EntityState.Deleted)
        {
            // Hard delete (entities that are not soft-deletable).
            action = AuditActions.Deleted;
            changes = new List<AuditFieldChange>();
        }
        else if (entry.State == EntityState.Added)
        {
            action = AuditActions.Created;
            changes = entry.Properties
                .Where(p => IsReportable(p.Metadata.Name) && p.CurrentValue != null)
                .Select(p => new AuditFieldChange(p.Metadata.Name, null, Format(p.Metadata.Name, p.CurrentValue)))
                .ToList();
        }
        else
        {
            var isDeletedProp = entry.Property(nameof(BaseEntity.IsDeleted));
            var wasDeleted = isDeletedProp.OriginalValue is true;
            var isDeleted = isDeletedProp.CurrentValue is true;

            if (!wasDeleted && isDeleted)
            {
                action = AuditActions.Deleted;
                changes = new List<AuditFieldChange>();
            }
            else if (wasDeleted && !isDeleted)
            {
                action = AuditActions.Restored;
                changes = new List<AuditFieldChange>();
            }
            else
            {
                action = AuditActions.Updated;
                changes = entry.Properties
                    .Where(p => p.IsModified
                                && IsReportable(p.Metadata.Name)
                                && !Equals(p.OriginalValue, p.CurrentValue))
                    .Select(p => new AuditFieldChange(
                        p.Metadata.Name,
                        Format(p.Metadata.Name, p.OriginalValue),
                        Format(p.Metadata.Name, p.CurrentValue)))
                    .ToList();

                // Nothing user-visible changed (e.g. only ModifiedDate touched) -> no audit row.
                if (changes.Count == 0)
                    return null;
            }
        }

        return new AuditLog
        {
            TenantId = entity.TenantId,
            BranchId = entity.BranchId,
            CreatedDate = now,
            CreatedBy = userId,
            Module = module,
            EntityName = clrType.Name,
            EntityId = entity.Id,
            EntityDisplay = Truncate(Describe(context, entity), 300),
            Action = action,
            ChangesJson = changes.Count > 0 ? JsonSerializer.Serialize(changes, JsonOptions) : null,
            ChangedFieldsCount = changes.Count,
            UserId = userId,
            UserName = userName,
            IpAddress = ip,
            Timestamp = now
        };
    }

    private static bool IsReportable(string propertyName)
    {
        if (IgnoredProperties.Contains(propertyName))
            return false;

        return !RedactedPropertyFragments.Any(f => propertyName.Contains(f, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Format(string propertyName, object? value)
    {
        if (value == null)
            return null;

        var text = value switch
        {
            byte[] => "[binary]",
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeSpan ts => ts.ToString("c", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

        return Truncate(text, MaxValueLength);
    }

    private static string? Truncate(string? text, int max)
    {
        if (text == null) return null;
        return text.Length <= max ? text : text[..max] + "…";
    }

    /// <summary>Best-effort human readable label for the audited record.</summary>
    private static string Describe(DbContext context, BaseEntity entity)
    {
        switch (entity)
        {
            case Employee e:
                return $"{e.EmployeeCode} - {e.FullNameEn}".Trim(' ', '-');
            case Attendance a:
                var employee = a.Employee ?? ResolveEmployee(context, a.EmployeeId);
                var who = employee != null ? $"{employee.EmployeeCode} - {employee.FullNameEn}" : a.EmployeeId.ToString();
                return $"{who} @ {a.Date:yyyy-MM-dd}";
            case EmployeeBiometric b:
                return b.EmployeeId.ToString();
            case EmployeeDocument d:
                return ReadString(d, "DocumentName") ?? ReadString(d, "FileName") ?? d.EmployeeId.ToString();
            case Organization o:
                return o.NameEn;
            case Branch br:
                return ReadString(br, "NameEn") ?? ReadString(br, "Name") ?? br.Id.ToString();
            case Department dep:
                return dep.NameEn;
            case JobTitle jt:
                return jt.TitleEn;
            case BranchHoliday h:
                return $"{ReadString(h, "NameEn") ?? ReadString(h, "Name") ?? "Holiday"} ({h.Date:yyyy-MM-dd})";
            case BranchWorkSchedule ws:
                return $"{ReadString(ws, "Name") ?? ReadString(ws, "NameEn") ?? "Work schedule"} ({ws.StartTime:hh\\:mm}-{ws.EndTime:hh\\:mm})";
            case BranchAttendanceSetting bas:
                return $"Attendance settings (branch {bas.BranchId})";
            case BranchCheckInPoint cp:
                return ReadString(cp, "Name") ?? ReadString(cp, "NameEn") ?? cp.Id.ToString();
            case BranchRequestSetting rs:
                return $"Request settings (branch {rs.BranchId})";
            case OrganizationPayrollSettings ps:
                return $"Payroll settings ({ps.CycleType})";
            default:
                return entity.Id.ToString();
        }
    }

    /// <summary>Finds the employee in the change tracker first, then (read-only) in the database.</summary>
    private static Employee? ResolveEmployee(DbContext context, Guid employeeId)
    {
        try
        {
            var tracked = context.ChangeTracker.Entries<Employee>()
                .Select(x => x.Entity)
                .FirstOrDefault(x => x.Id == employeeId);
            if (tracked != null) return tracked;

            return context.Set<Employee>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == employeeId);
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadString(object entity, string propertyName)
    {
        var prop = entity.GetType().GetProperty(propertyName);
        if (prop == null || prop.PropertyType != typeof(string))
            return null;

        var value = prop.GetValue(entity) as string;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? SafeUserName()
    {
        try
        {
            var name = CurrentUser.Name;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeIpAddress()
    {
        try
        {
            var http = CurrentUser.HttpContextAccessor?.HttpContext;
            if (http == null) return null;

            var forwarded = http.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();

            return http.Connection?.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public sealed record AuditFieldChange(string Field, string? OldValue, string? NewValue);
}
