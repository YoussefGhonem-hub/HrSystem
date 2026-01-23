namespace HrSystem.Shared.Constants;

public static class InvoiceStatusIds
{
    public static readonly Guid Pending = new Guid("00000000-0000-0000-000b-000000000001");
    public static readonly Guid Paid = new Guid("00000000-0000-0000-000b-000000000002");
    public static readonly Guid Overdue = new Guid("00000000-0000-0000-000b-000000000003");
    public static readonly Guid PartiallyPaid = new Guid("00000000-0000-0000-000b-000000000004");
    public static readonly Guid Cancelled = new Guid("00000000-0000-0000-000b-000000000005");
}
