using ErrorOr;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.CreateLeavePolicy;

public record CreateLeavePolicyCommand(
    Guid LeaveTypeId,
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

public record LeavePolicyDto
{
    public Guid Id { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeNameEn { get; set; } = string.Empty;
    public string LeaveTypeNameAr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int DefaultDaysPerYear { get; set; }
    public int MaxCarryForward { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool RequiresHRApproval { get; set; }
    public bool IsPaid { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinDaysNotice { get; set; }
    public bool RequiresDocument { get; set; }
    public string? Description { get; set; }
}

public class CreateLeavePolicyCommandHandler : IRequestHandler<CreateLeavePolicyCommand, ErrorOr<GenericResponse<LeavePolicyDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateLeavePolicyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeavePolicyDto>>> Handle(CreateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Validate leave type exists
        var leaveType = await _context.Set<LeaveType>()
            .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId && lt.IsActive, cancellationToken);

        if (leaveType == null)
        {
            return Error.NotFound(code: "LeaveType.NotFound", description: "Leave type not found");
        }

        // Check if policy already exists for this leave type
        var existingPolicy = await _context.Set<LeavePolicy>()
            .AnyAsync(lp => lp.LeaveTypeId == request.LeaveTypeId && lp.TenantId == orgId.Value, cancellationToken);

        if (existingPolicy)
        {
            return Error.Conflict(code: "LeavePolicy.AlreadyExists", description: "A leave policy already exists for this leave type");
        }

        var policy = new LeavePolicy
        {
            LeaveTypeId = request.LeaveTypeId,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DefaultDaysPerYear = request.DefaultDaysPerYear,
            MaxCarryForward = request.MaxCarryForward,
            RequiresApproval = request.RequiresApproval,
            RequiresManagerApproval = request.RequiresManagerApproval,
            RequiresHRApproval = request.RequiresHRApproval,
            IsPaid = request.IsPaid,
            MaxConsecutiveDays = request.MaxConsecutiveDays,
            MinDaysNotice = request.MinDaysNotice,
            RequiresDocument = request.RequiresDocument,
            Description = request.Description,
            TenantId = orgId.Value
        };

        policy.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        await _context.Set<LeavePolicy>().AddAsync(policy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new LeavePolicyDto
        {
            Id = policy.Id,
            LeaveTypeId = policy.LeaveTypeId,
            LeaveTypeNameEn = leaveType.NameEn,
            LeaveTypeNameAr = leaveType.NameAr,
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
            Message = "Leave policy created successfully",
            Data = dto
        };
    }
}
