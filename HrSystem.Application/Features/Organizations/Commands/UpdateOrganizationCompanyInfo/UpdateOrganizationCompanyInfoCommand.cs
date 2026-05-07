using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationCompanyInfo;

/// <summary>
/// Updates organization company profile information (org-company-info-tab).
/// Supports partial updates - only provided fields are updated.
/// </summary>
public record UpdateOrganizationCompanyInfoCommand(
    Guid OrganizationId,
    
    // Basic Info
    string? NameAr,
    string? NameEn,
    string? Industry,
    string? LogoUrl,
    
    // Legal Info
    string? CommercialRegistrationNumber,
    string? TaxRegistrationNumber,
    string? LegalEntityType,
    
    // Contact Info
    string? Email,
    string? PhoneNumber,
    string? SecondaryPhoneNumber,
    string? Website,
    string? AddressAr,
    string? AddressEn,
    string? City,
    string? Country,
    string? PostalCode,
    
    // Settings
    string? TimeZone,
    string? Currency,
    string? WeekStartDay,
    string? DefaultLanguage
) : IRequest<ErrorOr<GenericResponse<CompanyInfoDto>>>;

public record CompanyInfoDto
{
    public Guid OrganizationId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? LogoUrl { get; init; }
    public string? CommercialRegistrationNumber { get; init; }
    public string? TaxRegistrationNumber { get; init; }
    public string? LegalEntityType { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? SecondaryPhoneNumber { get; init; }
    public string? Website { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public string? TimeZone { get; init; }
    public string? Currency { get; init; }
    public string? WeekStartDay { get; init; }
    public string? DefaultLanguage { get; init; }
}

public class UpdateOrganizationCompanyInfoCommandHandler
    : IRequestHandler<UpdateOrganizationCompanyInfoCommand, ErrorOr<GenericResponse<CompanyInfoDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOrganizationCompanyInfoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<CompanyInfoDto>>> Handle(
        UpdateOrganizationCompanyInfoCommand request,
        CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && !o.IsDeleted, cancellationToken);

        if (organization == null)
            return Error.NotFound("Organization.NotFound", "Organization not found");

        // Update only provided fields
        if (request.NameAr != null) organization.NameAr = request.NameAr;
        if (request.NameEn != null) organization.NameEn = request.NameEn;
        if (request.Industry != null) organization.Industry = request.Industry;
        if (request.LogoUrl != null) organization.LogoUrl = request.LogoUrl;
        if (request.CommercialRegistrationNumber != null) organization.CommercialRegistrationNumber = request.CommercialRegistrationNumber;
        if (request.TaxRegistrationNumber != null) organization.TaxRegistrationNumber = request.TaxRegistrationNumber;
        if (request.LegalEntityType != null) organization.LegalEntityType = request.LegalEntityType;
        if (request.Email != null) organization.Email = request.Email;
        if (request.PhoneNumber != null) organization.PhoneNumber = request.PhoneNumber;
        if (request.SecondaryPhoneNumber != null) organization.SecondaryPhoneNumber = request.SecondaryPhoneNumber;
        if (request.Website != null) organization.Website = request.Website;
        if (request.AddressAr != null) organization.AddressAr = request.AddressAr;
        if (request.AddressEn != null) organization.AddressEn = request.AddressEn;
        if (request.City != null) organization.City = request.City;
        if (request.Country != null) organization.Country = request.Country;
        if (request.PostalCode != null) organization.PostalCode = request.PostalCode;
        if (request.TimeZone != null) organization.TimeZone = request.TimeZone;
        if (request.Currency != null) organization.Currency = request.Currency;
        if (request.WeekStartDay != null) organization.WeekStartDay = request.WeekStartDay;
        if (request.DefaultLanguage != null) organization.DefaultLanguage = request.DefaultLanguage;

        organization.ModifiedDate = DateTimeOffset.UtcNow;
        organization.ModifiedBy = CurrentUser.Id;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CompanyInfoDto
        {
            OrganizationId = organization.Id,
            Code = organization.Code,
            NameAr = organization.NameAr,
            NameEn = organization.NameEn,
            Industry = organization.Industry,
            LogoUrl = organization.LogoUrl,
            CommercialRegistrationNumber = organization.CommercialRegistrationNumber,
            TaxRegistrationNumber = organization.TaxRegistrationNumber,
            LegalEntityType = organization.LegalEntityType,
            Email = organization.Email,
            PhoneNumber = organization.PhoneNumber,
            SecondaryPhoneNumber = organization.SecondaryPhoneNumber,
            Website = organization.Website,
            AddressAr = organization.AddressAr,
            AddressEn = organization.AddressEn,
            City = organization.City,
            Country = organization.Country,
            PostalCode = organization.PostalCode,
            TimeZone = organization.TimeZone,
            Currency = organization.Currency,
            WeekStartDay = organization.WeekStartDay,
            DefaultLanguage = organization.DefaultLanguage
        };

        return new GenericResponse<CompanyInfoDto>
        {
            Success = true,
            Message = "Company information updated successfully",
            Data = dto
        };
    }
}
