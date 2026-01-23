namespace HrSystem.Shared.Constants;

public static class EmployeeStatusIds
{
    public static readonly Guid Active = new Guid("00000000-0000-0000-0008-000000000001");
    public static readonly Guid Probation = new Guid("00000000-0000-0000-0008-000000000002");
    public static readonly Guid OnLeave = new Guid("00000000-0000-0000-0008-000000000003");
    public static readonly Guid Suspended = new Guid("00000000-0000-0000-0008-000000000004");
    public static readonly Guid Terminated = new Guid("00000000-0000-0000-0008-000000000005");
    public static readonly Guid Resigned = new Guid("00000000-0000-0000-0008-000000000006");
}
