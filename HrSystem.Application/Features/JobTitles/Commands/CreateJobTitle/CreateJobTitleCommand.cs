using ErrorOr;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Commands.CreateJobTitle;

public record CreateJobTitleCommand(
    string TitleAr,
    string TitleEn,
    string? Description,
    int Level,
    decimal MinSalary,
    decimal MaxSalary
) : IRequest<ErrorOr<GenericResponse<JobTitleDto>>>;

public class CreateJobTitleCommandHandler : IRequestHandler<CreateJobTitleCommand, ErrorOr<GenericResponse<JobTitleDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateJobTitleCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<JobTitleDto>>> Handle(
        CreateJobTitleCommand request,
        CancellationToken cancellationToken)
    {
        var jobTitle = new JobTitle
        {
            TitleAr = request.TitleAr,
            TitleEn = request.TitleEn,
            Description = request.Description,
            Level = request.Level,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            BranchId = CurrentUser.BranchId
        };

        _context.JobTitles.Add(jobTitle);
        await _context.SaveChangesAsync(cancellationToken);

        var createdJobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstAsync(j => j.Id == jobTitle.Id, cancellationToken);

        var dto = new JobTitleDto
        {
            Id = createdJobTitle.Id,
            TitleAr = createdJobTitle.TitleAr,
            TitleEn = createdJobTitle.TitleEn,
            Description = createdJobTitle.Description,
            Level = createdJobTitle.Level,
            MinSalary = createdJobTitle.MinSalary,
            MaxSalary = createdJobTitle.MaxSalary,
            EmployeeCount = createdJobTitle.Employees.Count
        };

        return new GenericResponse<JobTitleDto>
        {
            Success = true,
            Message = "Job title created successfully",
            Data = dto
        };
    }
}
