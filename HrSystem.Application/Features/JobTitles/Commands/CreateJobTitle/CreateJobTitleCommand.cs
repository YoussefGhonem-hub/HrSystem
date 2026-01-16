using ErrorOr;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Commands.CreateJobTitle;

public record CreateJobTitleCommand(CreateJobTitleDto JobTitle) : IRequest<ErrorOr<GenericResponse<JobTitleDto>>>;

public class CreateJobTitleCommandHandler : IRequestHandler<CreateJobTitleCommand, ErrorOr<GenericResponse<JobTitleDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateJobTitleCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<JobTitleDto>>> Handle(
        CreateJobTitleCommand request,
        CancellationToken cancellationToken)
    {
        var jobTitle = request.JobTitle.Adapt<JobTitle>();
        jobTitle.TenantId = Guid.NewGuid(); // Should come from CurrentUser.OrganizationId

        _context.JobTitles.Add(jobTitle);
        await _context.SaveChangesAsync(cancellationToken);

        var createdJobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstAsync(j => j.Id == jobTitle.Id, cancellationToken);

        var dto = createdJobTitle.Adapt<JobTitleDto>();

        return new GenericResponse<JobTitleDto>
        {
            Success = true,
            Message = "Job title created successfully",
            Data = dto
        };
    }
}
