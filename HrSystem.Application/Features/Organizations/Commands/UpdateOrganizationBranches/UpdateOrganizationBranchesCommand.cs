using ErrorOr;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationBranches;

/// <summary>
/// Updates organization branches (org-branches-tab).
/// Supports add, update, and soft-delete operations via the Action field.
/// </summary>
public record UpdateOrganizationBranchesCommand(
    Guid OrganizationId,
    List<BranchUpdateInput> Branches
) : IRequest<ErrorOr<GenericResponse<BranchesUpdateDto>>>;

public record BranchUpdateInput(
    Guid? Id, // null for new branches
    BranchAction Action, // Add, Update, Delete
    
    // Basic Info
    string? NameAr,
    string? NameEn,
    string? Code,
    string? Description,
    
    // Location
    Guid? CountryId,
    string? City,
    string? AddressAr,
    string? AddressEn,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    
    // Contact
    string? PhoneNumber,
    string? Email,
    string? Fax,
    
    // Settings
    string? TimeZone,
    string? Currency,
    string? Language,
    
    // Status
    bool? IsHeadquarter,
    DateTime? OpeningDate
);

public enum BranchAction
{
    Add = 1,
    Update = 2,
    Delete = 3
}

public record BranchesUpdateDto
{
    public Guid OrganizationId { get; init; }
    public List<BranchDto> Branches { get; init; } = new();
    public int Added { get; init; }
    public int Updated { get; init; }
    public int Deleted { get; init; }
}

public record BranchDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CountryId { get; init; }
    public string? City { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? PostalCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Fax { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string? Language { get; init; }
    public bool IsHeadquarter { get; init; }
    public bool IsActive { get; init; }
    public DateTime? OpeningDate { get; init; }
}

public class UpdateOrganizationBranchesCommandHandler
    : IRequestHandler<UpdateOrganizationBranchesCommand, ErrorOr<GenericResponse<BranchesUpdateDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOrganizationBranchesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchesUpdateDto>>> Handle(
        UpdateOrganizationBranchesCommand request,
        CancellationToken cancellationToken)
    {
        // Verify organization exists
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && !o.IsDeleted, cancellationToken);

        if (organization == null)
            return Error.NotFound("Organization.NotFound", "Organization not found");

        // Get existing branches
        var existingBranches = await _context.Branches
            .Where(b => b.OrganizationId == request.OrganizationId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        int added = 0, updated = 0, deleted = 0;
        var newBranches = new List<Branch>();

        foreach (var input in request.Branches)
        {
            switch (input.Action)
            {
                case BranchAction.Add:
                    // Validate required fields for new branch
                    if (string.IsNullOrEmpty(input.NameAr) || string.IsNullOrEmpty(input.NameEn) || 
                        string.IsNullOrEmpty(input.Code) || !input.CountryId.HasValue)
                    {
                        return Error.Validation("Branch.MissingFields", 
                            "NameAr, NameEn, Code, and CountryId are required for new branches");
                    }

                    // Check code uniqueness
                    var codeExists = existingBranches.Any(b => b.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                     newBranches.Any(b => b.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                     await _context.Branches.AnyAsync(b => b.Code == input.Code && !b.IsDeleted, cancellationToken);
                    if (codeExists)
                        return Error.Conflict("Branch.CodeExists", $"Branch code '{input.Code}' already exists");

                    var newBranch = new Branch
                    {
                        Id = Guid.NewGuid(),
                        TenantId = organization.Id,
                        OrganizationId = organization.Id,
                        NameAr = input.NameAr,
                        NameEn = input.NameEn,
                        Code = input.Code,
                        Description = input.Description,
                        CountryId = input.CountryId.Value,
                        City = input.City,
                        AddressAr = input.AddressAr,
                        AddressEn = input.AddressEn,
                        PostalCode = input.PostalCode,
                        Latitude = input.Latitude,
                        Longitude = input.Longitude,
                        PhoneNumber = input.PhoneNumber,
                        Email = input.Email,
                        Fax = input.Fax,
                        TimeZone = input.TimeZone ?? organization.TimeZone,
                        Currency = input.Currency ?? organization.Currency,
                        Language = input.Language ?? "ar",
                        IsHeadquarter = input.IsHeadquarter ?? false,
                        IsActive = true,
                        OpeningDate = input.OpeningDate,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedBy = CurrentUser.Id
                    };
                    newBranches.Add(newBranch);
                    added++;
                    break;

                case BranchAction.Update:
                    if (!input.Id.HasValue)
                        return Error.Validation("Branch.IdRequired", "Branch ID is required for updates");

                    var branchToUpdate = existingBranches.FirstOrDefault(b => b.Id == input.Id.Value);
                    if (branchToUpdate == null)
                        return Error.NotFound("Branch.NotFound", $"Branch with ID '{input.Id}' not found");

                    // Check code uniqueness if code is being changed
                    if (input.Code != null && !input.Code.Equals(branchToUpdate.Code, StringComparison.OrdinalIgnoreCase))
                    {
                        var newCodeExists = existingBranches.Any(b => b.Id != input.Id && b.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                           newBranches.Any(b => b.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                           await _context.Branches.AnyAsync(b => b.Id != input.Id && b.Code == input.Code && !b.IsDeleted, cancellationToken);
                        if (newCodeExists)
                            return Error.Conflict("Branch.CodeExists", $"Branch code '{input.Code}' already exists");
                    }

                    // Update only provided fields
                    if (input.NameAr != null) branchToUpdate.NameAr = input.NameAr;
                    if (input.NameEn != null) branchToUpdate.NameEn = input.NameEn;
                    if (input.Code != null) branchToUpdate.Code = input.Code;
                    if (input.Description != null) branchToUpdate.Description = input.Description;
                    if (input.CountryId.HasValue) branchToUpdate.CountryId = input.CountryId.Value;
                    if (input.City != null) branchToUpdate.City = input.City;
                    if (input.AddressAr != null) branchToUpdate.AddressAr = input.AddressAr;
                    if (input.AddressEn != null) branchToUpdate.AddressEn = input.AddressEn;
                    if (input.PostalCode != null) branchToUpdate.PostalCode = input.PostalCode;
                    if (input.Latitude.HasValue) branchToUpdate.Latitude = input.Latitude;
                    if (input.Longitude.HasValue) branchToUpdate.Longitude = input.Longitude;
                    if (input.PhoneNumber != null) branchToUpdate.PhoneNumber = input.PhoneNumber;
                    if (input.Email != null) branchToUpdate.Email = input.Email;
                    if (input.Fax != null) branchToUpdate.Fax = input.Fax;
                    if (input.TimeZone != null) branchToUpdate.TimeZone = input.TimeZone;
                    if (input.Currency != null) branchToUpdate.Currency = input.Currency;
                    if (input.Language != null) branchToUpdate.Language = input.Language;
                    if (input.IsHeadquarter.HasValue) branchToUpdate.IsHeadquarter = input.IsHeadquarter.Value;
                    if (input.OpeningDate.HasValue) branchToUpdate.OpeningDate = input.OpeningDate;

                    branchToUpdate.ModifiedDate = DateTimeOffset.UtcNow;
                    branchToUpdate.ModifiedBy = CurrentUser.Id;
                    updated++;
                    break;

                case BranchAction.Delete:
                    if (!input.Id.HasValue)
                        return Error.Validation("Branch.IdRequired", "Branch ID is required for deletion");

                    var branchToDelete = existingBranches.FirstOrDefault(b => b.Id == input.Id.Value);
                    if (branchToDelete == null)
                        return Error.NotFound("Branch.NotFound", $"Branch with ID '{input.Id}' not found");

                    // Soft delete
                    branchToDelete.IsDeleted = true;
                    branchToDelete.DeletedDate = DateTimeOffset.UtcNow;
                    branchToDelete.DeletedBy = CurrentUser.Id;
                    branchToDelete.IsActive = false;
                    deleted++;
                    break;
            }
        }

        // Add new branches
        if (newBranches.Count > 0)
            await _context.Branches.AddRangeAsync(newBranches, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get all active branches for response
        var allBranches = await _context.Branches
            .Where(b => b.OrganizationId == request.OrganizationId && !b.IsDeleted)
            .OrderBy(b => b.IsHeadquarter ? 0 : 1)
            .ThenBy(b => b.CreatedDate)
            .ToListAsync(cancellationToken);

        var dto = new BranchesUpdateDto
        {
            OrganizationId = organization.Id,
            Added = added,
            Updated = updated,
            Deleted = deleted,
            Branches = allBranches.Select(b => new BranchDto
            {
                Id = b.Id,
                NameAr = b.NameAr,
                NameEn = b.NameEn,
                Code = b.Code,
                Description = b.Description,
                CountryId = b.CountryId,
                City = b.City,
                AddressAr = b.AddressAr,
                AddressEn = b.AddressEn,
                PostalCode = b.PostalCode,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                Fax = b.Fax,
                TimeZone = b.TimeZone,
                Currency = b.Currency,
                Language = b.Language,
                IsHeadquarter = b.IsHeadquarter,
                IsActive = b.IsActive,
                OpeningDate = b.OpeningDate
            }).ToList()
        };

        return new GenericResponse<BranchesUpdateDto>
        {
            Success = true,
            Message = $"Branches updated: {added} added, {updated} updated, {deleted} deleted",
            Data = dto
        };
    }
}
