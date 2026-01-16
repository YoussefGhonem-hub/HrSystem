namespace HrSystem.Application.Features.JobTitles.DTOs;

public record JobTitleDto
{
    public Guid Id { get; init; }
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
    public int EmployeeCount { get; init; }
    public DateTime CreatedDate { get; init; }
}

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

public record CreateJobTitleDto
{
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
}

public record UpdateJobTitleDto
{
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
}
