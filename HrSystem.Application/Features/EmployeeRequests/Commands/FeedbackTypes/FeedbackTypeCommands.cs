using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.FeedbackTypes;

#region Create FeedbackType
public record CreateFeedbackTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool IsAnonymousAllowed,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>;

public class CreateFeedbackTypeCommandHandler : IRequestHandler<CreateFeedbackTypeCommand, ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateFeedbackTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>> Handle(
        CreateFeedbackTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new FeedbackType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            IsAnonymousAllowed = request.IsAnonymousAllowed,
            RequiresManagerApproval = request.RequiresManagerApproval,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            TenantId = CurrentUser.OrganizationId ?? Guid.Empty,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.FeedbackTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new FeedbackTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsAnonymousAllowed = entity.IsAnonymousAllowed,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<FeedbackTypeDetailDto>.SuccessResult(dto, "Feedback type created successfully");
    }
}
#endregion

#region Update FeedbackType
public record UpdateFeedbackTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool IsAnonymousAllowed,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>;

public class UpdateFeedbackTypeCommandHandler : IRequestHandler<UpdateFeedbackTypeCommand, ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateFeedbackTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>> Handle(
        UpdateFeedbackTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.FeedbackTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Feedback type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.IsAnonymousAllowed = request.IsAnonymousAllowed;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new FeedbackTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsAnonymousAllowed = entity.IsAnonymousAllowed,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<FeedbackTypeDetailDto>.SuccessResult(dto, "Feedback type updated successfully");
    }
}
#endregion

#region Delete FeedbackType
public record DeleteFeedbackTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteFeedbackTypeCommandHandler : IRequestHandler<DeleteFeedbackTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteFeedbackTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteFeedbackTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.FeedbackTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Feedback type not found.");

        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Feedback type deleted successfully");
    }
}
#endregion
