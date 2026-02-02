using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Application.Features.EmployeeRequests.RequestTypes.Commands;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.RequestTypes.Queries;

public record GetRequestTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<RequestTypeDto>>>;

public class GetRequestTypeByIdQueryHandler : IRequestHandler<GetRequestTypeByIdQuery, ErrorOr<GenericResponse<RequestTypeDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetRequestTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<RequestTypeDto>>> Handle(GetRequestTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var entity = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.TenantId == orgId.Value && !r.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return Error.NotFound(description: "Request type not found");
        }

        return GenericResponse<RequestTypeDto>.SuccessResult(RequestTypeMapper.ToDto(entity));
    }
}
