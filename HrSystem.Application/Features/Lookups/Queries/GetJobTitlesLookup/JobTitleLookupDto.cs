namespace HrSystem.Application.Features.Lookups.Queries.GetJobTitlesLookup;

public class JobTitleLookupDto
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
}
