using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.VacationTypes;

#region Create VacationType
public record CreateVacationTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool IsPaid,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<VacationTypeDetailDto>>>;

public class CreateVacationTypeCommandHandler : IRequestHandler<CreateVacationTypeCommand, ErrorOr<GenericResponse<VacationTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateVacationTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<VacationTypeDetailDto>>> Handle(
        CreateVacationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new VacationType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            IsPaid = request.IsPaid,
            RequiresManagerApproval = request.RequiresManagerApproval,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.VacationTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new VacationTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsPaid = entity.IsPaid,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<VacationTypeDetailDto>.SuccessResult(dto, "Vacation type created successfully");
    }
}
#endregion

#region Update VacationType
public record UpdateVacationTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool IsPaid,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<VacationTypeDetailDto>>>;

public class UpdateVacationTypeCommandHandler : IRequestHandler<UpdateVacationTypeCommand, ErrorOr<GenericResponse<VacationTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateVacationTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<VacationTypeDetailDto>>> Handle(
        UpdateVacationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.VacationTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Vacation type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.IsPaid = request.IsPaid;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new VacationTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsPaid = entity.IsPaid,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<VacationTypeDetailDto>.SuccessResult(dto, "Vacation type updated successfully");
    }
}
#endregion

#region Delete VacationType
public record DeleteVacationTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteVacationTypeCommandHandler : IRequestHandler<DeleteVacationTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteVacationTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteVacationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.VacationTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Vacation type not found.");

        // Soft delete by setting IsActive = false
        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Vacation type deleted successfully");
    }
}
#endregion
