using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Extensions;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDocuments;

public record GetEmployeeDocumentDownloadUrlQuery(Guid DocumentId)
    : IRequest<ErrorOr<GenericResponse<string>>>;

public class GetEmployeeDocumentDownloadUrlQueryHandler : IRequestHandler<GetEmployeeDocumentDownloadUrlQuery, ErrorOr<GenericResponse<string>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public GetEmployeeDocumentDownloadUrlQueryHandler(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ErrorOr<GenericResponse<string>>> Handle(
        GetEmployeeDocumentDownloadUrlQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _context.EmployeeDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == request.DocumentId, cancellationToken);

        if (document == null)
        {
            return Error.NotFound("Documents.NotFound", "Document not found");
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
               CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
               CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != document.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to access document");
            }
        }

        var url = await _configuration.GetPreSignedUrlAsync(document.FilePath);
        if (string.IsNullOrWhiteSpace(url))
        {
            return Error.NotFound("Documents.NotFound", "Document URL not found");
        }

        return new GenericResponse<string>
        {
            Success = true,
            Message = "Document URL retrieved successfully",
            Data = url
        };
    }
}
