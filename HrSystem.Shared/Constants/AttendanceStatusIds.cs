namespace HrSystem.Shared.Constants;

public static class AttendanceStatusIds
{
    public static readonly Guid Present = new Guid("00000000-0000-0000-0004-000000000001");
    public static readonly Guid Absent = new Guid("00000000-0000-0000-0004-000000000002");
    public static readonly Guid Late = new Guid("00000000-0000-0000-0004-000000000003");
    public static readonly Guid EarlyLeave = new Guid("00000000-0000-0000-0004-000000000004");
    public static readonly Guid OnLeave = new Guid("00000000-0000-0000-0004-000000000005");
    public static readonly Guid Holiday = new Guid("00000000-0000-0000-0004-000000000006");
    public static readonly Guid Weekend = new Guid("00000000-0000-0000-0004-000000000007");
}
