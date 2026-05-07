using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetBranchAvailableRequests;

public record GetBranchAvailableRequestsQuery(Guid BranchId, bool IncludeHidden = false)
    : IRequest<ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>>;

public class GetBranchAvailableRequestsQueryHandler
    : IRequestHandler<GetBranchAvailableRequestsQuery, ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchAvailableRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchRequestAvailabilityDto>>>> Handle(
        GetBranchAvailableRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var settingsQuery = _context.BranchRequestSettings
            .AsNoTracking()
            .Include(s => s.RequestTypeRef)
            .Where(s => s.BranchId == request.BranchId);

        if (!request.IncludeHidden)
            settingsQuery = settingsQuery.Where(s => s.IsVisibleToEmployees);

        var settings = await settingsQuery
            .OrderBy(s => s.RequestTypeRef!.Code)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
            return Error.NotFound(description: "No request settings were found for the specified branch.");

        var requestTypeCodes = settings.Select(s => s.RequestTypeRef?.Code).Where(c => c != null).Distinct().ToList();

        // Load type-specific options based on request types enabled for this branch
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

        var attendanceCorrectionTypes = requestTypeCodes.Contains("AttendanceCorrection")
            ? await _context.AttendanceCorrectionTypes.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                .Select(t => new AttendanceCorrectionTypeDto
                {
                    Id = t.Id,
                    NameEn = t.NameEn,
                    NameAr = t.NameAr,
                    Description = t.Description,
                    RequiresManagerApproval = t.RequiresManagerApproval,
                    SortOrder = t.SortOrder
                }).ToListAsync(cancellationToken)
            : null;

        var dtos = settings
            .Select(setting => new BranchRequestAvailabilityDto
            {
                RequestTypeId = setting.RequestTypeId,
                RequestTypeCode = setting.RequestTypeRef?.Code ?? "",
                DisplayName = setting.RequestTypeRef?.Code ?? "",
                DisplayNameAr = setting.RequestTypeRef?.NameAr ?? "",
                IsVisibleToEmployees = setting.IsVisibleToEmployees,
                AllowEmployeesToSubmit = setting.AllowEmployeesToSubmit,
                RequireAttachment = setting.RequestTypeRef?.RequireAttachment ?? false,
                MaxOpenRequests = setting.MaxOpenRequests,
                CustomInstructions = setting.CustomInstructions,
                VacationTypes = setting.RequestTypeRef?.Code == "Vacation" ? vacationTypes : null,
                TrainingTypes = setting.RequestTypeRef?.Code == "Training" ? trainingTypes : null,
                MiscellaneousTypes = setting.RequestTypeRef?.Code == "Miscellaneous" ? miscellaneousTypes : null,
                PersonalTypes = setting.RequestTypeRef?.Code == "Personal" ? personalTypes : null,
                FeedbackTypes = setting.RequestTypeRef?.Code == "Feedback" ? feedbackTypes : null,
                AttendanceCorrectionTypes = setting.RequestTypeRef?.Code == "AttendanceCorrection" ? attendanceCorrectionTypes : null
            })
            .OrderBy(dto => dto.RequestTypeCode)
            .ToList();

        return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(dtos);
    }
}
