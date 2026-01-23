namespace HrSystem.Shared.Constants;

public static class GoalStatusIds
{
    public static readonly Guid NotStarted = new Guid("00000000-0000-0000-000d-000000000001");
    public static readonly Guid InProgress = new Guid("00000000-0000-0000-000d-000000000002");
    public static readonly Guid Completed = new Guid("00000000-0000-0000-000d-000000000003");
    public static readonly Guid OnHold = new Guid("00000000-0000-0000-000d-000000000004");
    public static readonly Guid Cancelled = new Guid("00000000-0000-0000-000d-000000000005");
}
