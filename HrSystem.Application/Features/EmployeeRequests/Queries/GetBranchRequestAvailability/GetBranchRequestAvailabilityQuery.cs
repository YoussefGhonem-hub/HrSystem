using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetBranchRequestAvailability;

public record GetBranchRequestAvailabilityQuery(Guid BranchId)
    : IRequest<ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>>;

public class GetBranchRequestAvailabilityQueryHandler
    : IRequestHandler<GetBranchRequestAvailabilityQuery, ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchRequestAvailabilityQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>> Handle(
        GetBranchRequestAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.BranchRequestSettings
            .AsNoTracking()
            .Where(s => s.BranchId == request.BranchId && s.IsVisibleToEmployees)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
        {
            return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(
                new List<BranchRequestAvailabilityDto>(),
                "No request types configured for this branch.");
        }

        var requestTypes = settings.Select(s => s.RequestType).Distinct().ToList();

        // Load type-specific options
        var vacationTypes = requestTypes.Contains(EmployeeRequestType.Vacation)
            ? await _context.VacationTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new VacationTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    IsPaid = t.IsPaid, MaxDaysPerYear = t.MaxDaysPerYear, RequiresAttachment = t.RequiresAttachment,
                    RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var overtimeTypes = requestTypes.Contains(EmployeeRequestType.OverTime)
            ? await _context.OvertimeTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new OvertimeTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    DefaultMultiplier = t.DefaultMultiplier, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var trainingTypes = requestTypes.Contains(EmployeeRequestType.Training)
            ? await _context.TrainingTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new TrainingTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresBudgetApproval = t.RequiresBudgetApproval, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var miscellaneousTypes = requestTypes.Contains(EmployeeRequestType.Miscellaneous)
            ? await _context.MiscellaneousTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new MiscellaneousTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresAttachment = t.RequiresAttachment, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var personalTypes = requestTypes.Contains(EmployeeRequestType.Personal)
            ? await _context.PersonalTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new PersonalTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresAttachment = t.RequiresAttachment, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var feedbackTypes = requestTypes.Contains(EmployeeRequestType.Feedback)
            ? await _context.FeedbackTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new FeedbackTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    IsAnonymousAllowed = t.IsAnonymousAllowed, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var permissionTypes = requestTypes.Contains(EmployeeRequestType.Permission)
            ? await _context.PermissionTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new PermissionTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    MaxHoursPerRequest = t.MaxHoursPerRequest, MaxHoursPerMonth = t.MaxHoursPerMonth,
                    DeductsFromLeave = t.DeductsFromLeave, HoursPerLeaveDay = t.HoursPerLeaveDay,
                    RequiresAttachment = t.RequiresAttachment, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var result = settings.Select(s => new BranchRequestAvailabilityDto
        {
            RequestType = s.RequestType,
            DisplayName = s.RequestType.ToString(),
            IsVisibleToEmployees = s.IsVisibleToEmployees,
            AllowEmployeesToSubmit = s.AllowEmployeesToSubmit,
            RequireAttachment = s.RequireAttachment,
            MaxOpenRequests = s.MaxOpenRequests,
            CustomInstructions = s.CustomInstructions,
            VacationTypes = s.RequestType == EmployeeRequestType.Vacation ? vacationTypes : null,
            OvertimeTypes = s.RequestType == EmployeeRequestType.OverTime ? overtimeTypes : null,
            TrainingTypes = s.RequestType == EmployeeRequestType.Training ? trainingTypes : null,
            MiscellaneousTypes = s.RequestType == EmployeeRequestType.Miscellaneous ? miscellaneousTypes : null,
            PersonalTypes = s.RequestType == EmployeeRequestType.Personal ? personalTypes : null,
            FeedbackTypes = s.RequestType == EmployeeRequestType.Feedback ? feedbackTypes : null,
            PermissionTypes = s.RequestType == EmployeeRequestType.Permission ? permissionTypes : null
        }).ToList();

        return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(result);
    }
}
