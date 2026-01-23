namespace HrSystem.Shared.Constants;

public static class PayrollStatusIds
{
    public static readonly Guid Draft = new Guid("00000000-0000-0000-0009-000000000001");
    public static readonly Guid Pending = new Guid("00000000-0000-0000-0009-000000000002");
    public static readonly Guid Approved = new Guid("00000000-0000-0000-0009-000000000003");
    public static readonly Guid Processed = new Guid("00000000-0000-0000-0009-000000000004");
    public static readonly Guid Paid = new Guid("00000000-0000-0000-0009-000000000005");
}
