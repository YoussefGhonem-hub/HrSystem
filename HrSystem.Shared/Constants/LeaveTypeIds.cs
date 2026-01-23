namespace HrSystem.Shared.Constants;

/// <summary>
/// Constants for Leave Type IDs used throughout the application
/// These IDs correspond to the seeded LeaveTypes table
/// </summary>
public static class LeaveTypeIds
{
    /// <summary>
    /// Annual Leave - Paid annual vacation leave
    /// </summary>
    public static readonly Guid Annual = new Guid("00000000-0000-0000-0001-000000000001");

    /// <summary>
    /// Sick Leave - Leave for illness or medical treatment
    /// </summary>
    public static readonly Guid Sick = new Guid("00000000-0000-0000-0001-000000000002");

    /// <summary>
    /// Emergency Leave - Leave for urgent personal matters
    /// </summary>
    public static readonly Guid Emergency = new Guid("00000000-0000-0000-0001-000000000003");

    /// <summary>
    /// Unpaid Leave - Leave without salary
    /// </summary>
    public static readonly Guid Unpaid = new Guid("00000000-0000-0000-0001-000000000004");

    /// <summary>
    /// Maternity Leave - Leave for childbirth and maternity
    /// </summary>
    public static readonly Guid Maternity = new Guid("00000000-0000-0000-0001-000000000005");

    /// <summary>
    /// Paternity Leave - Leave for fathers after childbirth
    /// </summary>
    public static readonly Guid Paternity = new Guid("00000000-0000-0000-0001-000000000006");

    /// <summary>
    /// Study Leave - Leave for educational purposes
    /// </summary>
    public static readonly Guid Study = new Guid("00000000-0000-0000-0001-000000000007");

    /// <summary>
    /// Bereavement Leave - Leave for death of a family member
    /// </summary>
    public static readonly Guid Bereavement = new Guid("00000000-0000-0000-0001-000000000008");

    /// <summary>
    /// Marriage Leave - Leave for wedding celebration
    /// </summary>
    public static readonly Guid Marriage = new Guid("00000000-0000-0000-0001-000000000009");

    /// <summary>
    /// Hajj Leave - Leave for Hajj pilgrimage
    /// </summary>
    public static readonly Guid Hajj = new Guid("00000000-0000-0000-0001-000000000010");
}
