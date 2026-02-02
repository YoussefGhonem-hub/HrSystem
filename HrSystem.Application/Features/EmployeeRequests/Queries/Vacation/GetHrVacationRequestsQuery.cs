using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;

public record GetHrVacationRequestsQuery(
    EmployeeRequestStatus? Status = null,
    Guid? VacationTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<VacationRequestListDto>>>>;

public class GetHrVacationRequestsQueryHandler : IRequestHandler<GetHrVacationRequestsQuery, ErrorOr<GenericResponse<PagedResult<VacationRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrVacationRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<VacationRequestListDto>>>> Handle(GetHrVacationRequestsQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);
        if (!isHr)
        {
            return Error.Forbidden("Vacation.HROnly", "Only HR roles can access branch vacation requests");
        }

        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
        {
            return Error.Conflict("Vacation.NoBranch", "Current HR user has no branch selected");
        }

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
            .Include(r => r.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Branch)
            .Include(r => r.VacationDetail)
                .ThenInclude(v => v!.VacationType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation")
            .Where(r => r.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.VacationTypeId.HasValue)
            query = query.Where(r => r.VacationDetail != null && r.VacationDetail.VacationTypeId == request.VacationTypeId.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= request.StartDateTo.Value);

        if (request.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

        // Sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new VacationRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                VacationTypeId = r.VacationDetail!.VacationTypeId,
                VacationTypeName = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameEn : null,
                VacationTypeNameAr = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameAr : null,
                StartDate = r.StartDate ?? DateTime.MinValue,
                EndDate = r.EndDate ?? DateTime.MinValue,
                TotalDays = r.VacationDetail.TotalDays,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
                ManagerComments = r.VacationDetail.ManagerComments,
                HRApprovalDate = r.VacationDetail.HRApprovalDate,
                HRComments = r.VacationDetail.HRComments,
                AttachmentUrl = r.AttachmentUrl,
                CreatedDate = r.CreatedDate,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? "Manager" :
                                       r.Status == EmployeeRequestStatus.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        var paged = new PagedResult<VacationRequestListDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<VacationRequestListDto>>
        {
            Success = true,
            Message = "HR vacation requests retrieved successfully",
            Data = paged
        };
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDescending
                ? query.OrderByDescending(r => r.StartDate)
                : query.OrderBy(r => r.StartDate);
        }

        return sortBy.ToLower() switch
        {
            "startdate" => sortDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate),
            "enddate" => sortDescending ? query.OrderByDescending(r => r.EndDate) : query.OrderBy(r => r.EndDate),
            "status" => sortDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "createddate" => sortDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate),
            "employeename" => sortDescending ? query.OrderByDescending(r => r.Employee.FullNameEn) : query.OrderBy(r => r.Employee.FullNameEn),
            _ => sortDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate)
        };
    }
}
