namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitlesList;

public record JobTitleListDto
{
    public Guid Id { get; init; }
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
    public int EmployeeCount { get; init; }
}
