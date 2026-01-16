using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, ErrorOr<GenericResponse<BranchDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<BranchDto>>> Handle(
        UpdateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(description: "Branch not found");
        }

        branch.NameAr = request.NameAr;
        branch.NameEn = request.NameEn;
        branch.Description = request.Description;
        branch.City = request.City;
        branch.AddressAr = request.AddressAr;
        branch.AddressEn = request.AddressEn;
        branch.PostalCode = request.PostalCode;
        branch.Latitude = request.Latitude;
        branch.Longitude = request.Longitude;
        branch.PhoneNumber = request.PhoneNumber;
        branch.Email = request.Email;
        branch.Fax = request.Fax;
        branch.TimeZone = request.TimeZone;
        branch.Currency = request.Currency;
        branch.Language = request.Language;
        branch.IsActive = request.IsActive;
        branch.BranchManagerId = request.BranchManagerId;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedBranch = await _context.Branches
            .Include(b => b.BranchManager)
            .Include(b => b.Employees)
            .Include(b => b.Departments)
            .FirstAsync(b => b.Id == branch.Id, cancellationToken);

        var dto = new BranchDto
        {
            Id = updatedBranch.Id,
            NameAr = updatedBranch.NameAr,
            NameEn = updatedBranch.NameEn,
            Code = updatedBranch.Code,
            Description = updatedBranch.Description,
            Country = updatedBranch.Country,
            City = updatedBranch.City,
            AddressAr = updatedBranch.AddressAr,
            AddressEn = updatedBranch.AddressEn,
            PostalCode = updatedBranch.PostalCode,
            Latitude = updatedBranch.Latitude,
            Longitude = updatedBranch.Longitude,
            PhoneNumber = updatedBranch.PhoneNumber,
            Email = updatedBranch.Email,
            Fax = updatedBranch.Fax,
            TimeZone = updatedBranch.TimeZone,
            Currency = updatedBranch.Currency,
            Language = updatedBranch.Language,
            IsHeadquarter = updatedBranch.IsHeadquarter,
            IsActive = updatedBranch.IsActive,
            OpeningDate = updatedBranch.OpeningDate,
            ClosingDate = updatedBranch.ClosingDate,
            BranchManagerId = updatedBranch.BranchManagerId,
            BranchManagerName = updatedBranch.BranchManager?.FullNameEn,
            EmployeeCount = updatedBranch.Employees.Count,
            DepartmentCount = updatedBranch.Departments.Count
        };

        return new GenericResponse<BranchDto>
        {
            Success = true,
            Message = "Branch updated successfully",
            Data = dto
        };
    }
}
