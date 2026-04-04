using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.CreateBranch;

public record CreateBranchCommand(
    string NameAr,
    string NameEn,
    string Code,
    string? Description,
    Guid CountryId,
    string? City,
    string? AddressAr,
    string? AddressEn,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? PhoneNumber,
    string? Email,
    string? Fax,
    string TimeZone,
    string Currency,
    string? Language,
    bool IsHeadquarter,
    DateTime? OpeningDate,
    Guid? BranchManagerId
) : IRequest<ErrorOr<GenericResponse<BranchDto>>>;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, ErrorOr<GenericResponse<BranchDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateBranchCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<BranchDto>>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var branch = new Branch
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Code = request.Code,
            Description = request.Description,
            CountryId = request.CountryId,
            City = request.City,
            AddressAr = request.AddressAr,
            AddressEn = request.AddressEn,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Fax = request.Fax,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            Language = request.Language,
            IsHeadquarter = request.IsHeadquarter,
            OpeningDate = request.OpeningDate,
            BranchManagerId = request.BranchManagerId,
            IsActive = true,
            OrganizationId = Guid.NewGuid(), // Should come from CurrentUser.OrganizationId
            TenantId = Guid.Empty
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        var createdBranch = await _context.Branches
            .Include(b => b.BranchManager)
            .Include(b => b.Employees)
            .Include(b => b.Departments)
            .Include(b => b.Country)
            .FirstAsync(b => b.Id == branch.Id, cancellationToken);

        var dto = new BranchDto
        {
            Id = createdBranch.Id,
            NameAr = createdBranch.NameAr,
            NameEn = createdBranch.NameEn,
            Code = createdBranch.Code,
            Description = createdBranch.Description,
            CountryId = createdBranch.CountryId,
            CountryNameEn = createdBranch.Country?.NameEn,
            CountryNameAr = createdBranch.Country?.NameAr,
            City = createdBranch.City,
            AddressAr = createdBranch.AddressAr,
            AddressEn = createdBranch.AddressEn,
            PostalCode = createdBranch.PostalCode,
            Latitude = createdBranch.Latitude,
            Longitude = createdBranch.Longitude,
            PhoneNumber = createdBranch.PhoneNumber,
            Email = createdBranch.Email,
            Fax = createdBranch.Fax,
            TimeZone = createdBranch.TimeZone,
            Currency = createdBranch.Currency,
            Language = createdBranch.Language,
            IsHeadquarter = createdBranch.IsHeadquarter,
            IsActive = createdBranch.IsActive,
            OpeningDate = createdBranch.OpeningDate,
            ClosingDate = createdBranch.ClosingDate,
            BranchManagerId = createdBranch.BranchManagerId,
            BranchManagerName = createdBranch.BranchManager?.FullNameEn,
            EmployeeCount = createdBranch.Employees.Count,
            DepartmentCount = createdBranch.Departments.Count
        };

        return new GenericResponse<BranchDto>
        {
            Success = true,
            Message = "Branch created successfully",
            Data = dto
        };
    }
}
