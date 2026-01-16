namespace HrSystem.Application.Features.JobTitles.Commands.UpdateJobTitle;

public record UpdateJobTitleDto
{
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
}
