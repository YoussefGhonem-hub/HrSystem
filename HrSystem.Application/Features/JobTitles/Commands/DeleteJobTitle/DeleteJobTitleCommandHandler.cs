using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Commands.DeleteJobTitle;

public class DeleteJobTitleCommandHandler : IRequestHandler<DeleteJobTitleCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteJobTitleCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteJobTitleCommand request,
        CancellationToken cancellationToken)
    {
        var jobTitle = await _context.JobTitles
            .Include(j => j.Employees)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (jobTitle == null)
        {
            return Error.NotFound(description: "Job title not found");
        }

        // Check if job title has employees
        if (jobTitle.Employees.Any())
        {
            return Error.Conflict(description: "Cannot delete job title with assigned employees");
        }

        _context.JobTitles.Remove(jobTitle);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Job title deleted successfully"
        };
    }
}
