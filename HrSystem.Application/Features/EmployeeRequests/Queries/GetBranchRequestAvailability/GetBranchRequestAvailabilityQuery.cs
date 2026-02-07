using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
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
            .Include(s => s.RequestTypeRef)
            .Where(s => s.BranchId == request.BranchId && s.IsVisibleToEmployees)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
        {
            return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(
                new List<BranchRequestAvailabilityDto>(),
                "No request types configured for this branch.");
        }

        var requestTypeCodes = settings.Select(s => s.RequestTypeRef?.Code).Where(c => c != null).Distinct().ToList();

        // Load type-specific options
        var vacationTypes = requestTypeCodes.Contains("Vacation")
            ? await _context.VacationTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new VacationTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    IsPaid = t.IsPaid, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var trainingTypes = requestTypeCodes.Contains("Training")
            ? await _context.TrainingTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new TrainingTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var miscellaneousTypes = requestTypeCodes.Contains("Miscellaneous")
            ? await _context.MiscellaneousTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new MiscellaneousTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var personalTypes = requestTypeCodes.Contains("Personal")
            ? await _context.PersonalTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new PersonalTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var feedbackTypes = requestTypeCodes.Contains("Feedback")
            ? await _context.FeedbackTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new FeedbackTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    IsAnonymousAllowed = t.IsAnonymousAllowed, RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var permissionTypes = requestTypeCodes.Contains("Permission")
            ? await _context.PermissionTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new PermissionTypeDto
                {
                    Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description,
                    RequiresManagerApproval = t.RequiresManagerApproval, SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var result = settings.Select(s => new BranchRequestAvailabilityDto
        {
            RequestTypeId = s.RequestTypeId,
            RequestTypeCode = s.RequestTypeRef?.Code ?? "",
            DisplayName = s.RequestTypeRef?.Code ?? "",
            DisplayNameAr = s.RequestTypeRef?.NameAr ?? "",
            IsVisibleToEmployees = s.IsVisibleToEmployees,
            AllowEmployeesToSubmit = s.AllowEmployeesToSubmit,
            RequireAttachment = s.RequireAttachment,
            MaxOpenRequests = s.MaxOpenRequests,
            CustomInstructions = s.CustomInstructions,
            VacationTypes = s.RequestTypeRef?.Code == "Vacation" ? vacationTypes : null,
            TrainingTypes = s.RequestTypeRef?.Code == "Training" ? trainingTypes : null,
            MiscellaneousTypes = s.RequestTypeRef?.Code == "Miscellaneous" ? miscellaneousTypes : null,
            PersonalTypes = s.RequestTypeRef?.Code == "Personal" ? personalTypes : null,
            FeedbackTypes = s.RequestTypeRef?.Code == "Feedback" ? feedbackTypes : null,
            PermissionTypes = s.RequestTypeRef?.Code == "Permission" ? permissionTypes : null
        }).ToList();

        return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(result);
    }
}
