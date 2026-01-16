using ErrorOr;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Commands.UpdateJobTitle;

public class UpdateJobTitleCommandHandler : IRequestHandler<UpdateJobTitleCommand, ErrorOr<GenericResponse<JobTitleDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateJobTitleCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<JobTitleDto>>> Handle(
        UpdateJobTitleCommand request,
        CancellationToken cancellationToken)
    {
        var jobTitle = await _context.JobTitles
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobTitle == null)
        {
            return Error.NotFound(description: "Job title not found");
        }

        jobTitle.TitleAr = request.TitleAr;
        jobTitle.TitleEn = request.TitleEn;
        jobTitle.Description = request.Description;
        jobTitle.Level = request.Level;
        jobTitle.MinSalary = request.MinSalary;
        jobTitle.MaxSalary = request.MaxSalary;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedJobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstAsync(j => j.Id == jobTitle.Id, cancellationToken);

        var dto = new JobTitleDto
        {
            Id = updatedJobTitle.Id,
            TitleAr = updatedJobTitle.TitleAr,
            TitleEn = updatedJobTitle.TitleEn,
            Description = updatedJobTitle.Description,
            Level = updatedJobTitle.Level,
            MinSalary = updatedJobTitle.MinSalary,
            MaxSalary = updatedJobTitle.MaxSalary,
            EmployeeCount = updatedJobTitle.Employees.Count
        };

        return new GenericResponse<JobTitleDto>
        {
            Success = true,
            Message = "Job title updated successfully",
            Data = dto
        };
    }
}
