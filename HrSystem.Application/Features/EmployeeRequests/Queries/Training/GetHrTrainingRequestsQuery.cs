using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Training;

public record GetHrTrainingRequestsQuery(
    EmployeeRequestStatus? Status = null,
    Guid? TrainingTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<TrainingRequestListDto>>>>;

public class GetHrTrainingRequestsQueryHandler : IRequestHandler<GetHrTrainingRequestsQuery, ErrorOr<GenericResponse<PagedResult<TrainingRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrTrainingRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<TrainingRequestListDto>>>> Handle(
        GetHrTrainingRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.TrainingTypeId.HasValue)
            query = query.Where(r => r.TrainingDetail != null && r.TrainingDetail.TrainingTypeId == request.TrainingTypeId.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.TrainingDetail != null && r.TrainingDetail.TrainingStartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.TrainingDetail != null && r.TrainingDetail.TrainingStartDate <= request.StartDateTo.Value);

        if (request.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new TrainingRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                TrainingTypeId = r.TrainingDetail!.TrainingTypeId,
                TrainingTypeName = r.TrainingDetail.TrainingType != null ? r.TrainingDetail.TrainingType.NameEn : null,
                TrainingTypeNameAr = r.TrainingDetail.TrainingType != null ? r.TrainingDetail.TrainingType.NameAr : null,
                TrainingName = r.TrainingDetail.TrainingName,
                TrainingProvider = r.TrainingDetail.TrainingProvider,
                TrainingLocation = r.TrainingDetail.TrainingLocation,
                TrainingStartDate = r.TrainingDetail.TrainingStartDate,
                TrainingEndDate = r.TrainingDetail.TrainingEndDate,
                DurationDays = r.TrainingDetail.DurationDays,
                EstimatedCost = r.TrainingDetail.EstimatedCost,
                ApprovedBudget = r.TrainingDetail.ApprovedBudget,
                Currency = r.TrainingDetail.Currency,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                RequestedDate = r.RequestedDate,
                ApprovedDate = r.ApprovedDate,
                RejectionReason = r.RejectionReason,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? 1 :
                                      r.Status == EmployeeRequestStatus.ManagerApproved ? 2 : 0
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<TrainingRequestListDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
        return GenericResponse<PagedResult<TrainingRequestListDto>>.SuccessResult(pagedResult, "HR training requests retrieved successfully");
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "trainingdate" => descending
                ? query.OrderByDescending(r => r.TrainingDetail!.TrainingStartDate)
                : query.OrderBy(r => r.TrainingDetail!.TrainingStartDate),
            "status" => descending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "employee" => descending
                ? query.OrderByDescending(r => r.Employee.FullNameEn)
                : query.OrderBy(r => r.Employee.FullNameEn),
            _ => query.OrderByDescending(r => r.RequestedDate)
        };
    }
}
