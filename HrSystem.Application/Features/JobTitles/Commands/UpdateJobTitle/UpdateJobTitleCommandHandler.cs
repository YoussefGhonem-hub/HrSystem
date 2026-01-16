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

        request.JobTitle.Adapt(jobTitle);

        await _context.SaveChangesAsync(cancellationToken);

        var updatedJobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstAsync(j => j.Id == jobTitle.Id, cancellationToken);

        var dto = updatedJobTitle.Adapt<JobTitleDto>();

        return new GenericResponse<JobTitleDto>
        {
            Success = true,
            Message = "Job title updated successfully",
            Data = dto
        };
    }
}
