using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Models;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDocuments;

/// <summary>
/// Query to download an employee document file directly
/// </summary>
public record DownloadEmployeeDocumentQuery(Guid DocumentId)
    : IRequest<ErrorOr<DownloadedFile>>;

public class DownloadEmployeeDocumentQueryHandler : IRequestHandler<DownloadEmployeeDocumentQuery, ErrorOr<DownloadedFile>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public DownloadEmployeeDocumentQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<DownloadedFile>> Handle(
        DownloadEmployeeDocumentQuery request,
        CancellationToken cancellationToken)
    {
        // Find the document
        var document = await _context.EmployeeDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == request.DocumentId, cancellationToken);

        if (document == null)
        {
            return Error.NotFound("Documents.NotFound", "Document not found");
        }

        // Authorization check - HR roles or document owner
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != document.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to access this document");
            }
        }

        // Check if S3 key exists (stored in FilePath)
        var s3Key = ExtractS3Key(document.FilePath);
        if (string.IsNullOrWhiteSpace(s3Key))
        {
            return Error.NotFound("Documents.FileNotFound", "Document S3 key not found");
        }

        // Download file from S3 using the key
        var downloadedFile = await _storageService.DownloadFile(s3Key, cancellationToken);

        if (downloadedFile == null || downloadedFile.Contents == null || downloadedFile.Contents.Length == 0)
        {
            return Error.Failure("Documents.DownloadFailed", "Failed to download document from storage");
        }

        // Return file with stored metadata
        return new DownloadedFile(
            downloadedFile.Contents,
            !string.IsNullOrWhiteSpace(document.ContentType) ? document.ContentType : downloadedFile.ContentType,
            !string.IsNullOrWhiteSpace(document.DocumentName) ? document.DocumentName : downloadedFile.FileName
        );
    }

    /// <summary>
    /// Extracts the S3 key from a file path or full S3 URL
    /// Example URL: https://bucket.s3.region.amazonaws.com/key.pdf?X-Amz-Expires=...
    /// Returns: key.pdf
    /// </summary>
    private static string? ExtractS3Key(string? filePathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(filePathOrUrl))
            return null;

        // If it's a full S3 URL, extract just the key
        if (filePathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(filePathOrUrl);
                // Get the path without query string and remove leading slash
                var path = uri.AbsolutePath.TrimStart('/');
                return path;
            }
            catch
            {
                // If URI parsing fails, return original
                return filePathOrUrl;
            }
        }

        // Already a key, return as-is
        return filePathOrUrl;
    }
}
