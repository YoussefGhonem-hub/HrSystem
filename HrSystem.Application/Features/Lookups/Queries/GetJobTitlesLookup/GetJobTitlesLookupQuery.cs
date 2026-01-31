using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetJobTitlesLookup;

public record GetJobTitlesLookupQuery : IRequest<ErrorOr<GenericResponse<List<JobTitleLookupDto>>>>;

public class GetJobTitlesLookupQueryHandler : IRequestHandler<GetJobTitlesLookupQuery, ErrorOr<GenericResponse<List<JobTitleLookupDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetJobTitlesLookupQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<JobTitleLookupDto>>>> Handle(
        GetJobTitlesLookupQuery request,
        CancellationToken cancellationToken)
    {
        var jobTitles = await _context.JobTitles
            .OrderBy(j => j.TitleEn)
            .Select(j => new JobTitleLookupDto
            {
                Id = j.Id,
                TitleEn = j.TitleEn,
                TitleAr = j.TitleAr
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<JobTitleLookupDto>>
        {
            Success = true,
            Message = "Job titles retrieved successfully",
            Data = jobTitles
        };
    }
}
