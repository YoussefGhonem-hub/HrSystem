namespace HrSystem.Application.Features.Lookups.Queries.GetBranchLookup;

public class BranchLookupDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
