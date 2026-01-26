using ErrorOr;
using HrSystem.Application.Features.Leave.Commands.CreateLeavePolicy;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetLeavePolicies;

public record GetLeavePoliciesQuery : IRequest<ErrorOr<GenericResponse<List<LeavePolicyDto>>>>;

public class GetLeavePoliciesQueryHandler : IRequestHandler<GetLeavePoliciesQuery, ErrorOr<GenericResponse<List<LeavePolicyDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeavePoliciesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<LeavePolicyDto>>>> Handle(GetLeavePoliciesQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var policies = await _context.Set<HrSystem.Domain.Entities.Leave.LeavePolicy>()
            .Include(lp => lp.LeaveType)
            .Where(lp => lp.TenantId == orgId.Value && !lp.IsDeleted)
            .OrderBy(lp => lp.LeaveType.DisplayOrder)
            .ThenBy(lp => lp.NameEn)
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
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<LeavePolicyDto>>
        {
            Success = true,
            Message = "Leave policies retrieved successfully",
            Data = policies
        };
    }
}
