using ErrorOr;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateBranchHolidays;

/// <summary>
/// Updates holidays for a branch (org-holidays-tab).
/// Supports add, update, and delete operations.
/// </summary>
public record UpdateBranchHolidaysCommand(
    Guid OrganizationId,
    Guid BranchId,
    List<HolidayUpdateInput> Holidays
) : IRequest<ErrorOr<GenericResponse<HolidaysUpdateDto>>>;

public record HolidayUpdateInput(
    Guid? Id,
    HolidayAction Action,
    string? NameAr,
    string? NameEn,
    string? Description,
    DateTime? Date,
    int? Year,
    bool? IsRecurring,
    int? RecurringMonth,
    int? RecurringDay,
    HolidayType? Type
);

public enum HolidayAction
{
    Add = 1,
    Update = 2,
    Delete = 3
}

public record HolidaysUpdateDto
{
    public Guid OrganizationId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public List<HolidayDto> Holidays { get; init; } = new();
    public int Added { get; init; }
    public int Updated { get; init; }
    public int Deleted { get; init; }
}

public record HolidayDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime Date { get; init; }
    public int Year { get; init; }
    public bool IsRecurring { get; init; }
    public int? RecurringMonth { get; init; }
    public int? RecurringDay { get; init; }
    public HolidayType Type { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateBranchHolidaysCommandHandler
    : IRequestHandler<UpdateBranchHolidaysCommand, ErrorOr<GenericResponse<HolidaysUpdateDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchHolidaysCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HolidaysUpdateDto>>> Handle(
        UpdateBranchHolidaysCommand request,
        CancellationToken cancellationToken)
    {
        // Verify organization and branch exist
        var branch = await _context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && 
                                      b.OrganizationId == request.OrganizationId && 
                                      !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch.NotFound", "Branch not found or does not belong to this organization");

        // Get existing holidays
        var existingHolidays = await _context.BranchHolidays
            .Where(h => h.BranchId == request.BranchId && !h.IsDeleted)
            .ToListAsync(cancellationToken);

        int added = 0, updated = 0, deleted = 0;
        var newHolidays = new List<BranchHoliday>();

        foreach (var input in request.Holidays)
        {
            switch (input.Action)
            {
                case HolidayAction.Add:
                    if (string.IsNullOrEmpty(input.NameAr) || string.IsNullOrEmpty(input.NameEn) || 
                        !input.Date.HasValue || !input.Year.HasValue)
                    {
                        return Error.Validation("Holiday.MissingFields", 
                            "NameAr, NameEn, Date, and Year are required for new holidays");
                    }

                    // Check for duplicate dates
                    var dateExists = existingHolidays.Any(h => h.Date.Date == input.Date.Value.Date) ||
                                    newHolidays.Any(h => h.Date.Date == input.Date.Value.Date);
                    if (dateExists)
                    {
                        return Error.Conflict("Holiday.DateExists", 
                            $"A holiday already exists for date {input.Date.Value:yyyy-MM-dd}");
                    }

                    var newHoliday = new BranchHoliday
                    {
                        Id = Guid.NewGuid(),
                        TenantId = branch.OrganizationId,
                        BranchId = branch.Id,
                        NameAr = input.NameAr,
                        NameEn = input.NameEn,
                        Description = input.Description,
                        Date = input.Date.Value,
                        Year = input.Year.Value,
                        IsRecurring = input.IsRecurring ?? false,
                        RecurringMonth = input.RecurringMonth,
                        RecurringDay = input.RecurringDay,
                        Type = input.Type ?? HolidayType.Public,
                        IsActive = true,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedBy = CurrentUser.Id
                    };
                    newHolidays.Add(newHoliday);
                    added++;
                    break;

                case HolidayAction.Update:
                    if (!input.Id.HasValue)
                        return Error.Validation("Holiday.IdRequired", "Holiday ID is required for updates");

                    var holidayToUpdate = existingHolidays.FirstOrDefault(h => h.Id == input.Id.Value);
                    if (holidayToUpdate == null)
                        return Error.NotFound("Holiday.NotFound", $"Holiday with ID '{input.Id}' not found");

                    // Check for duplicate dates if date is being changed
                    if (input.Date.HasValue && input.Date.Value.Date != holidayToUpdate.Date.Date)
                    {
                        var newDateExists = existingHolidays.Any(h => h.Id != input.Id && h.Date.Date == input.Date.Value.Date) ||
                                           newHolidays.Any(h => h.Date.Date == input.Date.Value.Date);
                        if (newDateExists)
                        {
                            return Error.Conflict("Holiday.DateExists", 
                                $"A holiday already exists for date {input.Date.Value:yyyy-MM-dd}");
                        }
                    }

                    if (input.NameAr != null) holidayToUpdate.NameAr = input.NameAr;
                    if (input.NameEn != null) holidayToUpdate.NameEn = input.NameEn;
                    if (input.Description != null) holidayToUpdate.Description = input.Description;
                    if (input.Date.HasValue) holidayToUpdate.Date = input.Date.Value;
                    if (input.Year.HasValue) holidayToUpdate.Year = input.Year.Value;
                    if (input.IsRecurring.HasValue) holidayToUpdate.IsRecurring = input.IsRecurring.Value;
                    if (input.RecurringMonth.HasValue) holidayToUpdate.RecurringMonth = input.RecurringMonth;
                    if (input.RecurringDay.HasValue) holidayToUpdate.RecurringDay = input.RecurringDay;
                    if (input.Type.HasValue) holidayToUpdate.Type = input.Type.Value;

                    holidayToUpdate.ModifiedDate = DateTimeOffset.UtcNow;
                    holidayToUpdate.ModifiedBy = CurrentUser.Id;
                    updated++;
                    break;

                case HolidayAction.Delete:
                    if (!input.Id.HasValue)
                        return Error.Validation("Holiday.IdRequired", "Holiday ID is required for deletion");

                    var holidayToDelete = existingHolidays.FirstOrDefault(h => h.Id == input.Id.Value);
                    if (holidayToDelete == null)
                        return Error.NotFound("Holiday.NotFound", $"Holiday with ID '{input.Id}' not found");

                    holidayToDelete.IsDeleted = true;
                    holidayToDelete.DeletedDate = DateTimeOffset.UtcNow;
                    holidayToDelete.DeletedBy = CurrentUser.Id;
                    holidayToDelete.IsActive = false;
                    deleted++;
                    break;
            }
        }

        // Add new holidays
        if (newHolidays.Count > 0)
            await _context.BranchHolidays.AddRangeAsync(newHolidays, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get all active holidays for response
        var allHolidays = await _context.BranchHolidays
            .Where(h => h.BranchId == request.BranchId && !h.IsDeleted)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

        var dto = new HolidaysUpdateDto
        {
            OrganizationId = branch.OrganizationId,
            BranchId = branch.Id,
            BranchName = branch.NameEn,
            Added = added,
            Updated = updated,
            Deleted = deleted,
            Holidays = allHolidays.Select(h => new HolidayDto
            {
                Id = h.Id,
                NameAr = h.NameAr,
                NameEn = h.NameEn,
                Description = h.Description,
                Date = h.Date,
                Year = h.Year,
                IsRecurring = h.IsRecurring,
                RecurringMonth = h.RecurringMonth,
                RecurringDay = h.RecurringDay,
                Type = h.Type,
                IsActive = h.IsActive
            }).ToList()
        };

        return new GenericResponse<HolidaysUpdateDto>
        {
            Success = true,
            Message = $"Holidays updated: {added} added, {updated} updated, {deleted} deleted",
            Data = dto
        };
    }
}
