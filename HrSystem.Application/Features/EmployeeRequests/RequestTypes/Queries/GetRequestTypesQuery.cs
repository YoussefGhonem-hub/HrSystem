using System.Collections.Generic;
using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.RequestTypes.Queries;

public record GetRequestTypesQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<RequestTypeDto>>>>;

public class GetRequestTypesQueryHandler : IRequestHandler<GetRequestTypesQuery, ErrorOr<GenericResponse<List<RequestTypeDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetRequestTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<RequestTypeDto>>>> Handle(GetRequestTypesQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var query = _context.RequestTypes
            .AsNoTracking()
            .Where(r => r.TenantId == orgId.Value && !r.IsDeleted);

        if (!request.IncludeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        var items = await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.NameEn)
            .Select(r => new RequestTypeDto
            {
                Id = r.Id,
                Code = r.Code,
                NameAr = r.NameAr,
                NameEn = r.NameEn,
                Description = r.Description,
                IsActive = r.IsActive,
                SortOrder = r.SortOrder,
                CreatedDate = r.CreatedDate,
                ModifiedDate = r.ModifiedDate
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<RequestTypeDto>>.SuccessResult(items);
    }
}
