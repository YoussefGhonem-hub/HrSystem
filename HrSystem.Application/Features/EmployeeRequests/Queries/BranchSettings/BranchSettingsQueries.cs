using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.BranchSettings;

#region Get All Branch Request Settings
public record GetBranchRequestSettingsQuery(
    Guid? BranchId = null,
    EmployeeRequestType? RequestType = null
) : IRequest<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>;

public class GetBranchRequestSettingsQueryHandler : IRequestHandler<GetBranchRequestSettingsQuery, ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchRequestSettingsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>> Handle(
        GetBranchRequestSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.BranchRequestSettings
            .Include(s => s.Branch)
            .AsQueryable();

        if (request.BranchId.HasValue)
            query = query.Where(s => s.BranchId == request.BranchId.Value);

        if (request.RequestType.HasValue)
            query = query.Where(s => s.RequestType == request.RequestType.Value);

        var entities = await query
            .OrderBy(s => s.Branch!.NameEn)
            .ThenBy(s => s.RequestType)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new BranchRequestSettingDetailDto
        {
            Id = e.Id,
            BranchId = e.BranchId ?? Guid.Empty,
            BranchName = e.Branch?.NameEn,
            RequestType = e.RequestType,
            RequestTypeName = e.RequestType.ToString(),
            IsVisibleToEmployees = e.IsVisibleToEmployees,
            AllowEmployeesToSubmit = e.AllowEmployeesToSubmit,
            RequireAttachment = e.RequireAttachment,
            MaxOpenRequests = e.MaxOpenRequests,
            CustomInstructions = e.CustomInstructions,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<BranchRequestSettingDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get Branch Request Setting By Id
public record GetBranchRequestSettingByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>;

public class GetBranchRequestSettingByIdQueryHandler : IRequestHandler<GetBranchRequestSettingByIdQuery, ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchRequestSettingByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>> Handle(
        GetBranchRequestSettingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.BranchRequestSettings
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (entity == null)
            return Error.NotFound(description: "Branch request setting not found.");

        var dto = new BranchRequestSettingDetailDto
        {
            Id = entity.Id,
            BranchId = entity.BranchId ?? Guid.Empty,
            BranchName = entity.Branch?.NameEn,
            RequestType = entity.RequestType,
            RequestTypeName = entity.RequestType.ToString(),
            IsVisibleToEmployees = entity.IsVisibleToEmployees,
            AllowEmployeesToSubmit = entity.AllowEmployeesToSubmit,
            RequireAttachment = entity.RequireAttachment,
            MaxOpenRequests = entity.MaxOpenRequests,
            CustomInstructions = entity.CustomInstructions,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<BranchRequestSettingDetailDto>.SuccessResult(dto);
    }
}
#endregion

#region Get Branches Without Settings
public record GetBranchesWithoutSettingsQuery() : IRequest<ErrorOr<GenericResponse<List<BranchSummaryDto>>>>;

public record BranchSummaryDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public int ConfiguredRequestTypesCount { get; init; }
    public int MissingRequestTypesCount { get; init; }
}

public class GetBranchesWithoutSettingsQueryHandler : IRequestHandler<GetBranchesWithoutSettingsQuery, ErrorOr<GenericResponse<List<BranchSummaryDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchesWithoutSettingsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchSummaryDto>>>> Handle(
        GetBranchesWithoutSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var totalRequestTypes = Enum.GetValues<EmployeeRequestType>().Length;

        var branches = await _context.Branches
            .Include(b => b.RequestSettings)
            .Select(b => new BranchSummaryDto
            {
                Id = b.Id,
                NameEn = b.NameEn,
                NameAr = b.NameAr,
                ConfiguredRequestTypesCount = b.RequestSettings.Count,
                MissingRequestTypesCount = totalRequestTypes - b.RequestSettings.Count
            })
            .OrderByDescending(b => b.MissingRequestTypesCount)
            .ThenBy(b => b.NameEn)
            .ToListAsync(cancellationToken);

        return GenericResponse<List<BranchSummaryDto>>.SuccessResult(branches);
    }
}
#endregion
