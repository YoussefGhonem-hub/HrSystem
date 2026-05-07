using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.AttendanceCorrectionTypes;

#region Create AttendanceCorrectionType
public record CreateAttendanceCorrectionTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>;

public class CreateAttendanceCorrectionTypeCommandHandler : IRequestHandler<CreateAttendanceCorrectionTypeCommand, ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateAttendanceCorrectionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>> Handle(
        CreateAttendanceCorrectionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new AttendanceCorrectionType
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

        _context.AttendanceCorrectionTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new AttendanceCorrectionTypeDetailDto
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

        return GenericResponse<AttendanceCorrectionTypeDetailDto>.SuccessResult(dto, "Attendance correction type created successfully");
    }
}
#endregion

#region Update AttendanceCorrectionType
public record UpdateAttendanceCorrectionTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>;

public class UpdateAttendanceCorrectionTypeCommandHandler : IRequestHandler<UpdateAttendanceCorrectionTypeCommand, ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateAttendanceCorrectionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>> Handle(
        UpdateAttendanceCorrectionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.AttendanceCorrectionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Attendance correction type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.RequireAttachment = request.RequireAttachment;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new AttendanceCorrectionTypeDetailDto
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

        return GenericResponse<AttendanceCorrectionTypeDetailDto>.SuccessResult(dto, "Attendance correction type updated successfully");
    }
}
#endregion

#region Delete AttendanceCorrectionType
public record DeleteAttendanceCorrectionTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteAttendanceCorrectionTypeCommandHandler : IRequestHandler<DeleteAttendanceCorrectionTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteAttendanceCorrectionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteAttendanceCorrectionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.AttendanceCorrectionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Attendance correction type not found.");

        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Attendance correction type deleted successfully");
    }
}
#endregion
