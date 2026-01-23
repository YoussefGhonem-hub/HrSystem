namespace HrSystem.Shared.Constants;

/// <summary>
/// Constants for Leave Status IDs used throughout the application
/// These IDs correspond to the seeded LeaveStatuses table
/// </summary>
public static class LeaveStatusIds
{
    /// <summary>
    /// Pending - Awaiting manager approval
    /// </summary>
    public static readonly Guid Pending = new Guid("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Manager Approved - Approved by manager, awaiting HR approval
    /// </summary>
    public static readonly Guid ManagerApproved = new Guid("00000000-0000-0000-0000-000000000002");

    /// <summary>
    /// HR Approved - Approved by HR, awaiting final approval
    /// </summary>
    public static readonly Guid HRApproved = new Guid("00000000-0000-0000-0000-000000000003");

    /// <summary>
    /// Approved - Fully approved and active
    /// </summary>
    public static readonly Guid Approved = new Guid("00000000-0000-0000-0000-000000000004");

    /// <summary>
    /// Rejected - Rejected by manager or HR
    /// </summary>
    public static readonly Guid Rejected = new Guid("00000000-0000-0000-0000-000000000005");

    /// <summary>
    /// Cancelled - Cancelled by employee
    /// </summary>
    public static readonly Guid Cancelled = new Guid("00000000-0000-0000-0000-000000000006");
}
