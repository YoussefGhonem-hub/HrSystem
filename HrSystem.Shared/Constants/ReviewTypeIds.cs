namespace HrSystem.Shared.Constants;

/// <summary>
/// Constants for Review Type IDs used throughout the application
/// These IDs correspond to the seeded ReviewTypes table
/// </summary>
public static class ReviewTypeIds
{
    /// <summary>
    /// Annual Review - Comprehensive annual performance review
    /// </summary>
    public static readonly Guid Annual = new Guid("00000000-0000-0000-0002-000000000001");

    /// <summary>
    /// Quarterly Review - Quarterly performance review
    /// </summary>
    public static readonly Guid Quarterly = new Guid("00000000-0000-0000-0002-000000000002");

    /// <summary>
    /// Probation Review - Employee probation period review
    /// </summary>
    public static readonly Guid Probation = new Guid("00000000-0000-0000-0002-000000000003");

    /// <summary>
    /// Mid-Year Review - Mid-year performance review
    /// </summary>
    public static readonly Guid MidYear = new Guid("00000000-0000-0000-0002-000000000004");

    /// <summary>
    /// Project Review - Post-project performance review
    /// </summary>
    public static readonly Guid Project = new Guid("00000000-0000-0000-0002-000000000005");
}
