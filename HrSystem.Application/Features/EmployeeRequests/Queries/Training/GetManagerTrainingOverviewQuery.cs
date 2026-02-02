using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Training;

public record GetManagerTrainingOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerTrainingOverviewDto>>>;

public class ManagerTrainingOverviewDto
{
    public PagedResult<TrainingRequestListDto> MyRequests { get; set; } = null!;
    public PagedResult<TrainingRequestListDto> PendingApprovals { get; set; } = null!;
}

public class GetManagerTrainingOverviewQueryHandler : IRequestHandler<GetManagerTrainingOverviewQuery, ErrorOr<GenericResponse<ManagerTrainingOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerTrainingOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerTrainingOverviewDto>>> Handle(
        GetManagerTrainingOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // Manager's own requests
        var myQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training")
            .Where(r => r.EmployeeId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var myTotal = await myQuery.CountAsync(cancellationToken);
        var myItems = await myQuery
            .Skip((request.MyPageNumber - 1) * request.MyPageSize)
            .Take(request.MyPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        // Pending approvals from direct reports
        var pendingQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training")
            .Where(r => r.Status == EmployeeRequestStatus.Pending)
            .Where(r => r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var pendingTotal = await pendingQuery.CountAsync(cancellationToken);
        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        var overview = new ManagerTrainingOverviewDto
        {
            MyRequests = PagedResult<TrainingRequestListDto>.Create(myItems, myTotal, request.MyPageNumber, request.MyPageSize),
            PendingApprovals = PagedResult<TrainingRequestListDto>.Create(pendingItems, pendingTotal, request.PendingPageNumber, request.PendingPageSize)
        };

        return GenericResponse<ManagerTrainingOverviewDto>.SuccessResult(overview, "Manager training overview retrieved successfully");
    }

    private static TrainingRequestListDto MapToDto(Domain.Entities.Requests.EmployeeRequest r)
    {
        return new TrainingRequestListDto
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
        };
    }
}
