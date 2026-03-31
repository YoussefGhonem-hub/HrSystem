using ErrorOr;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Payroll.Commands.CreateBankExportProfile;

public record CreateBankExportProfileCommand(
    string ProfileName,
    string BankName,
    string CompanyAccountNumber,
    string CompanyAccountName,
    string Currency,
    string? BicCode,
    string Narrative,
    bool IsDefault
) : IRequest<ErrorOr<GenericResponse<BankExportProfileDto>>>;

public class BankExportProfileDto
{
    public Guid Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string CompanyAccountNumber { get; set; } = string.Empty;
    public string CompanyAccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string? BicCode { get; set; }
    public string Narrative { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? TemplateFileName { get; set; }
    public bool HasTemplate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}

public class CreateBankExportProfileCommandHandler
    : IRequestHandler<CreateBankExportProfileCommand, ErrorOr<GenericResponse<BankExportProfileDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateBankExportProfileCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BankExportProfileDto>>> Handle(
        CreateBankExportProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
            return Error.Validation(description: "Profile name is required.");
        if (string.IsNullOrWhiteSpace(request.CompanyAccountNumber))
            return Error.Validation(description: "Company account number is required.");

        // If setting as default, unset other defaults in the same branch
        if (request.IsDefault)
        {
            var existingDefaults = _context.BankExportProfiles
                .Where(b => !b.IsDeleted && b.IsDefault);
            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        var profile = new BankExportProfile
        {
            ProfileName = request.ProfileName,
            BankName = request.BankName,
            CompanyAccountNumber = request.CompanyAccountNumber,
            CompanyAccountName = request.CompanyAccountName,
            Currency = request.Currency,
            BicCode = request.BicCode,
            Narrative = string.IsNullOrWhiteSpace(request.Narrative) ? "Salary" : request.Narrative,
            IsDefault = request.IsDefault
        };

        _context.BankExportProfiles.Add(profile);
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

        return GenericResponse<BankExportProfileDto>.SuccessResult(dto, "Bank export profile created successfully.");
    }
}
