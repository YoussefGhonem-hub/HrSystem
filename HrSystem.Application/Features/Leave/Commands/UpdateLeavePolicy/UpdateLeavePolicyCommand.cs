using ErrorOr;
using HrSystem.Application.Features.Leave.Commands.CreateLeavePolicy;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.UpdateLeavePolicy;

public record UpdateLeavePolicyCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    int DefaultDaysPerYear,
    int MaxCarryForward,
    bool RequiresApproval,
    bool RequiresManagerApproval,
    bool RequiresHRApproval,
    bool IsPaid,
    int MaxConsecutiveDays,
    int MinDaysNotice,
    bool RequiresDocument,
    string? Description
) : IRequest<ErrorOr<GenericResponse<LeavePolicyDto>>>;

public class UpdateLeavePolicyCommandHandler : IRequestHandler<UpdateLeavePolicyCommand, ErrorOr<GenericResponse<LeavePolicyDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateLeavePolicyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeavePolicyDto>>> Handle(UpdateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var policy = await _context.Set<HrSystem.Domain.Entities.Leave.LeavePolicy>()
            .Include(lp => lp.LeaveType)
            .FirstOrDefaultAsync(lp => lp.Id == request.Id && lp.TenantId == orgId.Value, cancellationToken);

        if (policy == null)
        {
            return Error.NotFound(code: "LeavePolicy.NotFound", description: "Leave policy not found");
        }

        policy.NameAr = request.NameAr;
        policy.NameEn = request.NameEn;
        policy.DefaultDaysPerYear = request.DefaultDaysPerYear;
        policy.MaxCarryForward = request.MaxCarryForward;
        policy.RequiresApproval = request.RequiresApproval;
        policy.RequiresManagerApproval = request.RequiresManagerApproval;
        policy.RequiresHRApproval = request.RequiresHRApproval;
        policy.IsPaid = request.IsPaid;
        policy.MaxConsecutiveDays = request.MaxConsecutiveDays;
        policy.MinDaysNotice = request.MinDaysNotice;
        policy.RequiresDocument = request.RequiresDocument;
        policy.Description = request.Description;

        policy.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new LeavePolicyDto
        {
            Id = policy.Id,
            LeaveTypeId = policy.LeaveTypeId,
            LeaveTypeNameEn = policy.LeaveType.NameEn,
            LeaveTypeNameAr = policy.LeaveType.NameAr,
            NameAr = policy.NameAr,
            NameEn = policy.NameEn,
            DefaultDaysPerYear = policy.DefaultDaysPerYear,
            MaxCarryForward = policy.MaxCarryForward,
            RequiresApproval = policy.RequiresApproval,
            RequiresManagerApproval = policy.RequiresManagerApproval,
            RequiresHRApproval = policy.RequiresHRApproval,
            IsPaid = policy.IsPaid,
            MaxConsecutiveDays = policy.MaxConsecutiveDays,
            MinDaysNotice = policy.MinDaysNotice,
            RequiresDocument = policy.RequiresDocument,
            Description = policy.Description
        };

        return new GenericResponse<LeavePolicyDto>
        {
            Success = true,
            Message = "Leave policy updated successfully",
            Data = dto
        };
    }
}
