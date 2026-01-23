namespace HrSystem.Shared.Constants;

/// <summary>
/// Constants for Review Status IDs used throughout the application
/// These IDs correspond to the seeded ReviewStatuses table
/// </summary>
public static class ReviewStatusIds
{
    /// <summary>
    /// Draft - Review is being prepared
    /// </summary>
    public static readonly Guid Draft = new Guid("00000000-0000-0000-0003-000000000001");

    /// <summary>
    /// Submitted - Review has been submitted
    /// </summary>
    public static readonly Guid Submitted = new Guid("00000000-0000-0000-0003-000000000002");

    /// <summary>
    /// Approved - Review has been approved
    /// </summary>
    public static readonly Guid Approved = new Guid("00000000-0000-0000-0003-000000000003");

    /// <summary>
    /// Completed - Review is completed and finalized
    /// </summary>
    public static readonly Guid Completed = new Guid("00000000-0000-0000-0003-000000000004");

    /// <summary>
    /// Rejected - Review has been rejected
    /// </summary>
    public static readonly Guid Rejected = new Guid("00000000-0000-0000-0003-000000000005");
}
