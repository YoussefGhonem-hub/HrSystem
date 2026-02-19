using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.TrainingTypes;

#region Create TrainingType
public record CreateTrainingTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<TrainingTypeDetailDto>>>;

public class CreateTrainingTypeCommandHandler : IRequestHandler<CreateTrainingTypeCommand, ErrorOr<GenericResponse<TrainingTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateTrainingTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<TrainingTypeDetailDto>>> Handle(
        CreateTrainingTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new TrainingType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            RequiresManagerApproval = request.RequiresManagerApproval,
            RequireAttachment = request.RequireAttachment,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.TrainingTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new TrainingTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<TrainingTypeDetailDto>.SuccessResult(dto, "Training type created successfully");
    }
}
#endregion

#region Update TrainingType
public record UpdateTrainingTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<TrainingTypeDetailDto>>>;

public class UpdateTrainingTypeCommandHandler : IRequestHandler<UpdateTrainingTypeCommand, ErrorOr<GenericResponse<TrainingTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateTrainingTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<TrainingTypeDetailDto>>> Handle(
        UpdateTrainingTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.TrainingTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Training type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.RequireAttachment = request.RequireAttachment;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new TrainingTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<TrainingTypeDetailDto>.SuccessResult(dto, "Training type updated successfully");
    }
}
#endregion

#region Delete TrainingType
public record DeleteTrainingTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteTrainingTypeCommandHandler : IRequestHandler<DeleteTrainingTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteTrainingTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteTrainingTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.TrainingTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Training type not found.");

        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Training type deleted successfully");
    }
}
#endregion
