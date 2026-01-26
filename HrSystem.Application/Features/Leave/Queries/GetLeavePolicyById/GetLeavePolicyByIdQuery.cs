using ErrorOr;
using HrSystem.Application.Features.Leave.Commands.CreateLeavePolicy;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetLeavePolicyById;

public record GetLeavePolicyByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<LeavePolicyDto>>>;

public class GetLeavePolicyByIdQueryHandler : IRequestHandler<GetLeavePolicyByIdQuery, ErrorOr<GenericResponse<LeavePolicyDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeavePolicyByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeavePolicyDto>>> Handle(GetLeavePolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var policy = await _context.Set<HrSystem.Domain.Entities.Leave.LeavePolicy>()
            .Include(lp => lp.LeaveType)
            .Where(lp => lp.Id == request.Id && lp.TenantId == orgId.Value && !lp.IsDeleted)
            .Select(lp => new LeavePolicyDto
            {
                Id = lp.Id,
                LeaveTypeId = lp.LeaveTypeId,
                LeaveTypeNameEn = lp.LeaveType.NameEn,
                LeaveTypeNameAr = lp.LeaveType.NameAr,
                NameAr = lp.NameAr,
                NameEn = lp.NameEn,
                DefaultDaysPerYear = lp.DefaultDaysPerYear,
                MaxCarryForward = lp.MaxCarryForward,
                RequiresApproval = lp.RequiresApproval,
                RequiresManagerApproval = lp.RequiresManagerApproval,
                RequiresHRApproval = lp.RequiresHRApproval,
                IsPaid = lp.IsPaid,
                MaxConsecutiveDays = lp.MaxConsecutiveDays,
                MinDaysNotice = lp.MinDaysNotice,
                RequiresDocument = lp.RequiresDocument,
                Description = lp.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null)
        {
            return Error.NotFound(code: "LeavePolicy.NotFound", description: "Leave policy not found");
        }

        return new GenericResponse<LeavePolicyDto>
        {
            Success = true,
            Message = "Leave policy retrieved successfully",
            Data = policy
        };
    }
}
