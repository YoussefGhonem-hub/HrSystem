using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
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
            .Where(s => s.BranchId == request.BranchId);

        if (!request.IncludeHidden)
            settingsQuery = settingsQuery.Where(s => s.IsVisibleToEmployees);

        var settings = await settingsQuery
            .OrderBy(s => s.RequestType)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
            return Error.NotFound(description: "No request settings were found for the specified branch.");

        var requestTypes = settings.Select(s => s.RequestType).Distinct().ToList();

        // Load type-specific options based on request types enabled for this branch
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

        var dtos = settings
            .Select(setting => new BranchRequestAvailabilityDto
            {
                RequestType = setting.RequestType,
                DisplayName = setting.RequestType.ToString(),
                IsVisibleToEmployees = setting.IsVisibleToEmployees,
                AllowEmployeesToSubmit = setting.AllowEmployeesToSubmit,
                RequireAttachment = setting.RequireAttachment,
                MaxOpenRequests = setting.MaxOpenRequests,
                CustomInstructions = setting.CustomInstructions,
                VacationTypes = setting.RequestType == EmployeeRequestType.Vacation ? vacationTypes : null,
                OvertimeTypes = setting.RequestType == EmployeeRequestType.OverTime ? overtimeTypes : null,
                TrainingTypes = setting.RequestType == EmployeeRequestType.Training ? trainingTypes : null,
                MiscellaneousTypes = setting.RequestType == EmployeeRequestType.Miscellaneous ? miscellaneousTypes : null,
                PersonalTypes = setting.RequestType == EmployeeRequestType.Personal ? personalTypes : null,
                FeedbackTypes = setting.RequestType == EmployeeRequestType.Feedback ? feedbackTypes : null
            })
            .OrderBy(dto => dto.RequestType)
            .ToList();

        return GenericResponse<List<BranchRequestAvailabilityDto>>.SuccessResult(dtos);
    }
}
