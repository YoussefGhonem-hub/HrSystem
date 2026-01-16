using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;

public class GetJobTitleByIdQueryHandler : IRequestHandler<GetJobTitleByIdQuery, ErrorOr<GenericResponse<JobTitleDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetJobTitleByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<JobTitleDto>>> Handle(
        GetJobTitleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var jobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobTitle == null)
        {
            return Error.NotFound(description: "Job title not found");
        }

        var dto = jobTitle.Adapt<JobTitleDto>();

        return new GenericResponse<JobTitleDto>
        {
            Success = true,
            Data = dto
        };
    }
}
