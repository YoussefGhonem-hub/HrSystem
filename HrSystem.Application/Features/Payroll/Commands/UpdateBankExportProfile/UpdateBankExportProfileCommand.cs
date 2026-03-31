using ErrorOr;
using HrSystem.Application.Features.Payroll.Commands.CreateBankExportProfile;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.UpdateBankExportProfile;

public record UpdateBankExportProfileCommand(
    Guid Id,
    string ProfileName,
    string BankName,
    string CompanyAccountNumber,
    string CompanyAccountName,
    string Currency,
    string? BicCode,
    string Narrative,
    bool IsDefault
) : IRequest<ErrorOr<GenericResponse<BankExportProfileDto>>>;

public class UpdateBankExportProfileCommandHandler
    : IRequestHandler<UpdateBankExportProfileCommand, ErrorOr<GenericResponse<BankExportProfileDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBankExportProfileCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BankExportProfileDto>>> Handle(
        UpdateBankExportProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.BankExportProfiles
            .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, cancellationToken);

        if (profile == null)
            return Error.NotFound(description: "Bank export profile not found.");

        // If setting as default, unset other defaults
        if (request.IsDefault && !profile.IsDefault)
        {
            var existingDefaults = _context.BankExportProfiles
                .Where(b => !b.IsDeleted && b.IsDefault && b.Id != request.Id);
            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        profile.ProfileName = request.ProfileName;
        profile.BankName = request.BankName;
        profile.CompanyAccountNumber = request.CompanyAccountNumber;
        profile.CompanyAccountName = request.CompanyAccountName;
        profile.Currency = request.Currency;
        profile.BicCode = request.BicCode;
        profile.Narrative = string.IsNullOrWhiteSpace(request.Narrative) ? "Salary" : request.Narrative;
        profile.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new BankExportProfileDto
        {
            Id = profile.Id,
            ProfileName = profile.ProfileName,
            BankName = profile.BankName,
            CompanyAccountNumber = profile.CompanyAccountNumber,
            CompanyAccountName = profile.CompanyAccountName,
            Currency = profile.Currency,
            BicCode = profile.BicCode,
            Narrative = profile.Narrative,
            IsDefault = profile.IsDefault,
            TemplateFileName = profile.TemplateFileName,
            HasTemplate = !string.IsNullOrEmpty(profile.TemplateFileKey),
            CreatedDate = profile.CreatedDate
        };

        return GenericResponse<BankExportProfileDto>.SuccessResult(dto, "Bank export profile updated successfully.");
    }
}
