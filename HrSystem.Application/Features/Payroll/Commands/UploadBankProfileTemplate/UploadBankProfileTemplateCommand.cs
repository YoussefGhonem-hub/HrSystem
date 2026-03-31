using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Payroll.Commands.UploadBankProfileTemplate;

public record UploadBankProfileTemplateCommand(
    Guid ProfileId,
    IFormFile TemplateFile
) : IRequest<ErrorOr<GenericResponse<UploadBankProfileTemplateResult>>>;

public class UploadBankProfileTemplateResult
{
    public Guid ProfileId { get; set; }
    public string TemplateFileName { get; set; } = string.Empty;
}

public class UploadBankProfileTemplateCommandHandler
    : IRequestHandler<UploadBankProfileTemplateCommand, ErrorOr<GenericResponse<UploadBankProfileTemplateResult>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UploadBankProfileTemplateCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<UploadBankProfileTemplateResult>>> Handle(
        UploadBankProfileTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.BankExportProfiles
            .FirstOrDefaultAsync(b => b.Id == request.ProfileId && !b.IsDeleted, cancellationToken);

        if (profile == null)
            return Error.NotFound(description: "Bank export profile not found.");

        if (request.TemplateFile == null || request.TemplateFile.Length == 0)
            return Error.Validation(description: "Template file is required.");

        var extension = Path.GetExtension(request.TemplateFile.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
            return Error.Validation(description: "Only Excel files (.xlsx, .xls) are allowed.");

        // Delete old template if exists
        if (!string.IsNullOrEmpty(profile.TemplateFileKey))
        {
            await _storageService.Delete(profile.TemplateFileKey, cancellationToken);
        }

        // Upload new template
        var storedFile = await _storageService.Upload(request.TemplateFile, cancellationToken);

        profile.TemplateFileKey = storedFile.Key;
        profile.TemplateFileName = request.TemplateFile.FileName;

        await _context.SaveChangesAsync(cancellationToken);

        var result = new UploadBankProfileTemplateResult
        {
            ProfileId = profile.Id,
            TemplateFileName = profile.TemplateFileName
        };

        return GenericResponse<UploadBankProfileTemplateResult>.SuccessResult(result, "Template uploaded successfully.");
    }
}
