using ErrorOr;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationStructure;

/// <summary>
/// Updates organization structure - departments and job titles (org-structure-tab).
/// Supports add, update, and soft-delete operations.
/// </summary>
public record UpdateOrganizationStructureCommand(
    Guid OrganizationId,
    List<DepartmentUpdateInput>? Departments,
    List<JobTitleUpdateInput>? JobTitles
) : IRequest<ErrorOr<GenericResponse<StructureUpdateDto>>>;

public record DepartmentUpdateInput(
    Guid? Id,
    StructureAction Action,
    string? NameAr,
    string? NameEn,
    string? Code,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? BranchId, // Optional: department can be org-wide or branch-specific
    int? SortOrder
);

public record JobTitleUpdateInput(
    Guid? Id,
    StructureAction Action,
    string? TitleAr,
    string? TitleEn,
    string? Code,
    string? Description,
    int? Level,
    decimal? MinSalary,
    decimal? MaxSalary,
    Guid? BranchId,
    int? SortOrder
);

public enum StructureAction
{
    Add = 1,
    Update = 2,
    Delete = 3
}

public record StructureUpdateDto
{
    public Guid OrganizationId { get; init; }
    public List<DepartmentDto> Departments { get; init; } = new();
    public List<JobTitleDto> JobTitles { get; init; } = new();
    public StructureStats Stats { get; init; } = new();
}

public record StructureStats
{
    public int DepartmentsAdded { get; init; }
    public int DepartmentsUpdated { get; init; }
    public int DepartmentsDeleted { get; init; }
    public int JobTitlesAdded { get; init; }
    public int JobTitlesUpdated { get; init; }
    public int JobTitlesDeleted { get; init; }
}

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record JobTitleDto
{
    public Guid Id { get; init; }
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
    public Guid? BranchId { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateOrganizationStructureCommandHandler
    : IRequestHandler<UpdateOrganizationStructureCommand, ErrorOr<GenericResponse<StructureUpdateDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOrganizationStructureCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<StructureUpdateDto>>> Handle(
        UpdateOrganizationStructureCommand request,
        CancellationToken cancellationToken)
    {
        // Verify organization exists
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && !o.IsDeleted, cancellationToken);

        if (organization == null)
            return Error.NotFound("Organization.NotFound", "Organization not found");

        var stats = new StructureStats();
        var newDepartments = new List<Department>();
        var newJobTitles = new List<JobTitle>();

        // Process Departments (PUT semantics: when provided, payload is the full desired state)
        if (request.Departments is not null)
        {
            var existingDepts = await _context.Departments
                .Where(d => d.OrganizationId == request.OrganizationId && !d.IsDeleted)
                .ToListAsync(cancellationToken);

            int deptAdded = 0, deptUpdated = 0, deptDeleted = 0;
            var incomingDepartmentIds = request.Departments
                .Where(d => d.Id.HasValue)
                .Select(d => d.Id!.Value)
                .ToHashSet();

            foreach (var input in request.Departments)
            {
                switch (input.Action)
                {
                    case StructureAction.Add:
                        if (string.IsNullOrEmpty(input.NameAr) || string.IsNullOrEmpty(input.NameEn) || string.IsNullOrEmpty(input.Code))
                            return Error.Validation("Department.MissingFields", "NameAr, NameEn, and Code are required for new departments");

                        var deptCodeExists = existingDepts.Any(d => d.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                            newDepartments.Any(d => d.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase));
                        if (deptCodeExists)
                            return Error.Conflict("Department.CodeExists", $"Department code '{input.Code}' already exists");

                        var newDept = new Department
                        {
                            Id = Guid.NewGuid(),
                            TenantId = organization.Id,
                            BranchId = input.BranchId,
                            OrganizationId = organization.Id,
                            NameAr = input.NameAr,
                            NameEn = input.NameEn,
                            Code = input.Code,
                            Description = input.Description,
                            ParentDepartmentId = input.ParentDepartmentId,
                            SortOrder = input.SortOrder ?? 0,
                            IsActive = true,
                            CreatedDate = DateTimeOffset.UtcNow,
                            CreatedBy = CurrentUser.Id
                        };
                        newDepartments.Add(newDept);
                        deptAdded++;
                        break;

                    case StructureAction.Update:
                        if (!input.Id.HasValue)
                            return Error.Validation("Department.IdRequired", "Department ID is required for updates");

                        var deptToUpdate = existingDepts.FirstOrDefault(d => d.Id == input.Id.Value);
                        if (deptToUpdate == null)
                            return Error.NotFound("Department.NotFound", $"Department with ID '{input.Id}' not found");

                        if (input.Code != null && !input.Code.Equals(deptToUpdate.Code, StringComparison.OrdinalIgnoreCase))
                        {
                            var newCodeExists = existingDepts.Any(d => d.Id != input.Id && d.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase));
                            if (newCodeExists)
                                return Error.Conflict("Department.CodeExists", $"Department code '{input.Code}' already exists");
                        }

                        if (input.NameAr != null) deptToUpdate.NameAr = input.NameAr;
                        if (input.NameEn != null) deptToUpdate.NameEn = input.NameEn;
                        if (input.Code != null) deptToUpdate.Code = input.Code;
                        if (input.Description != null) deptToUpdate.Description = input.Description;
                        if (input.ParentDepartmentId.HasValue) deptToUpdate.ParentDepartmentId = input.ParentDepartmentId;
                        if (input.BranchId.HasValue) deptToUpdate.BranchId = input.BranchId;
                        if (input.SortOrder.HasValue) deptToUpdate.SortOrder = input.SortOrder.Value;

                        deptToUpdate.ModifiedDate = DateTimeOffset.UtcNow;
                        deptToUpdate.ModifiedBy = CurrentUser.Id;
                        deptUpdated++;
                        break;

                    case StructureAction.Delete:
                        if (!input.Id.HasValue)
                            return Error.Validation("Department.IdRequired", "Department ID is required for deletion");

                        var deptToDelete = existingDepts.FirstOrDefault(d => d.Id == input.Id.Value);
                        if (deptToDelete == null)
                            return Error.NotFound("Department.NotFound", $"Department with ID '{input.Id}' not found");

                        deptToDelete.IsDeleted = true;
                        deptToDelete.DeletedDate = DateTimeOffset.UtcNow;
                        deptToDelete.DeletedBy = CurrentUser.Id;
                        deptToDelete.IsActive = false;
                        deptDeleted++;
                        break;
                }
            }

            // Implicit delete: any existing department not included in payload is considered removed.
            var implicitDeletedDepartments = existingDepts
                .Where(d => !incomingDepartmentIds.Contains(d.Id))
                .ToList();

            foreach (var dept in implicitDeletedDepartments)
            {
                dept.IsDeleted = true;
                dept.DeletedDate = DateTimeOffset.UtcNow;
                dept.DeletedBy = CurrentUser.Id;
                dept.IsActive = false;
                deptDeleted++;
            }

            stats = stats with { DepartmentsAdded = deptAdded, DepartmentsUpdated = deptUpdated, DepartmentsDeleted = deptDeleted };
        }

        // Process Job Titles (PUT semantics: when provided, payload is the full desired state)
        if (request.JobTitles is not null)
        {
            var existingTitles = await _context.JobTitles
                .Where(j => j.OrganizationId == request.OrganizationId && !j.IsDeleted)
                .ToListAsync(cancellationToken);

            int titleAdded = 0, titleUpdated = 0, titleDeleted = 0;
            var incomingJobTitleIds = request.JobTitles
                .Where(j => j.Id.HasValue)
                .Select(j => j.Id!.Value)
                .ToHashSet();

            foreach (var input in request.JobTitles)
            {
                switch (input.Action)
                {
                    case StructureAction.Add:
                        if (string.IsNullOrEmpty(input.TitleAr) || string.IsNullOrEmpty(input.TitleEn) || string.IsNullOrEmpty(input.Code))
                            return Error.Validation("JobTitle.MissingFields", "TitleAr, TitleEn, and Code are required for new job titles");

                        var titleCodeExists = existingTitles.Any(j => j.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase)) ||
                                             newJobTitles.Any(j => j.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase));
                        if (titleCodeExists)
                            return Error.Conflict("JobTitle.CodeExists", $"Job title code '{input.Code}' already exists");

                        var newTitle = new JobTitle
                        {
                            Id = Guid.NewGuid(),
                            TenantId = organization.Id,
                            BranchId = input.BranchId,
                            OrganizationId = organization.Id,
                            TitleAr = input.TitleAr,
                            TitleEn = input.TitleEn,
                            Code = input.Code,
                            Description = input.Description,
                            Level = input.Level ?? 1,
                            MinSalary = input.MinSalary ?? 0,
                            MaxSalary = input.MaxSalary ?? 0,
                            SortOrder = input.SortOrder ?? 0,
                            IsActive = true,
                            CreatedDate = DateTimeOffset.UtcNow,
                            CreatedBy = CurrentUser.Id
                        };
                        newJobTitles.Add(newTitle);
                        titleAdded++;
                        break;

                    case StructureAction.Update:
                        if (!input.Id.HasValue)
                            return Error.Validation("JobTitle.IdRequired", "Job title ID is required for updates");

                        var titleToUpdate = existingTitles.FirstOrDefault(j => j.Id == input.Id.Value);
                        if (titleToUpdate == null)
                            return Error.NotFound("JobTitle.NotFound", $"Job title with ID '{input.Id}' not found");

                        if (input.Code != null && !input.Code.Equals(titleToUpdate.Code, StringComparison.OrdinalIgnoreCase))
                        {
                            var newCodeExists = existingTitles.Any(j => j.Id != input.Id && j.Code.Equals(input.Code, StringComparison.OrdinalIgnoreCase));
                            if (newCodeExists)
                                return Error.Conflict("JobTitle.CodeExists", $"Job title code '{input.Code}' already exists");
                        }

                        if (input.TitleAr != null) titleToUpdate.TitleAr = input.TitleAr;
                        if (input.TitleEn != null) titleToUpdate.TitleEn = input.TitleEn;
                        if (input.Code != null) titleToUpdate.Code = input.Code;
                        if (input.Description != null) titleToUpdate.Description = input.Description;
                        if (input.Level.HasValue) titleToUpdate.Level = input.Level.Value;
                        if (input.MinSalary.HasValue) titleToUpdate.MinSalary = input.MinSalary.Value;
                        if (input.MaxSalary.HasValue) titleToUpdate.MaxSalary = input.MaxSalary.Value;
                        if (input.BranchId.HasValue) titleToUpdate.BranchId = input.BranchId;
                        if (input.SortOrder.HasValue) titleToUpdate.SortOrder = input.SortOrder.Value;

                        titleToUpdate.ModifiedDate = DateTimeOffset.UtcNow;
                        titleToUpdate.ModifiedBy = CurrentUser.Id;
                        titleUpdated++;
                        break;

                    case StructureAction.Delete:
                        if (!input.Id.HasValue)
                            return Error.Validation("JobTitle.IdRequired", "Job title ID is required for deletion");

                        var titleToDelete = existingTitles.FirstOrDefault(j => j.Id == input.Id.Value);
                        if (titleToDelete == null)
                            return Error.NotFound("JobTitle.NotFound", $"Job title with ID '{input.Id}' not found");

                        titleToDelete.IsDeleted = true;
                        titleToDelete.DeletedDate = DateTimeOffset.UtcNow;
                        titleToDelete.DeletedBy = CurrentUser.Id;
                        titleToDelete.IsActive = false;
                        titleDeleted++;
                        break;
                }
            }

            // Implicit delete: any existing job title not included in payload is considered removed.
            var implicitDeletedJobTitles = existingTitles
                .Where(j => !incomingJobTitleIds.Contains(j.Id))
                .ToList();

            foreach (var jobTitle in implicitDeletedJobTitles)
            {
                jobTitle.IsDeleted = true;
                jobTitle.DeletedDate = DateTimeOffset.UtcNow;
                jobTitle.DeletedBy = CurrentUser.Id;
                jobTitle.IsActive = false;
                titleDeleted++;
            }

            stats = stats with { JobTitlesAdded = titleAdded, JobTitlesUpdated = titleUpdated, JobTitlesDeleted = titleDeleted };
        }

        // Save new entities
        if (newDepartments.Count > 0)
            await _context.Departments.AddRangeAsync(newDepartments, cancellationToken);
        if (newJobTitles.Count > 0)
            await _context.JobTitles.AddRangeAsync(newJobTitles, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get all active entities for response
        var allDepartments = await _context.Departments
            .Where(d => d.OrganizationId == request.OrganizationId && !d.IsDeleted)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.NameEn)
            .ToListAsync(cancellationToken);

        var allJobTitles = await _context.JobTitles
            .Where(j => j.OrganizationId == request.OrganizationId && !j.IsDeleted)
            .OrderBy(j => j.SortOrder)
            .ThenBy(j => j.Level)
            .ThenBy(j => j.TitleEn)
            .ToListAsync(cancellationToken);

        var dto = new StructureUpdateDto
        {
            OrganizationId = organization.Id,
            Stats = stats,
            Departments = allDepartments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                NameAr = d.NameAr,
                NameEn = d.NameEn,
                Code = d.Code,
                Description = d.Description,
                ParentDepartmentId = d.ParentDepartmentId,
                BranchId = d.BranchId,
                SortOrder = d.SortOrder,
                IsActive = d.IsActive
            }).ToList(),
            JobTitles = allJobTitles.Select(j => new JobTitleDto
            {
                Id = j.Id,
                TitleAr = j.TitleAr,
                TitleEn = j.TitleEn,
                Code = j.Code,
                Description = j.Description,
                Level = j.Level,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                BranchId = j.BranchId,
                SortOrder = j.SortOrder,
                IsActive = j.IsActive
            }).ToList()
        };

        return new GenericResponse<StructureUpdateDto>
        {
            Success = true,
            Message = $"Structure updated: Departments ({stats.DepartmentsAdded} added, {stats.DepartmentsUpdated} updated, {stats.DepartmentsDeleted} deleted), " +
                     $"Job Titles ({stats.JobTitlesAdded} added, {stats.JobTitlesUpdated} updated, {stats.JobTitlesDeleted} deleted)",
            Data = dto
        };
    }
}
