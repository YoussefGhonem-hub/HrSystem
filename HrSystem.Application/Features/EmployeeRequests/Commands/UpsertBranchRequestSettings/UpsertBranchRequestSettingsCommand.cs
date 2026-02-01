using ErrorOr;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.UpsertBranchRequestSettings;

public record BranchRequestSettingPayload(
    EmployeeRequestType RequestType,
    bool IsVisibleToEmployees,
    bool AllowEmployeesToSubmit,
    bool RequireAttachment,
    int? MaxOpenRequests,
    string? CustomInstructions);

public record UpsertBranchRequestSettingsCommand(
    Guid BranchId,
    IReadOnlyCollection<BranchRequestSettingPayload> Settings
) : IRequest<ErrorOr<GenericResponse>>;

public class UpsertBranchRequestSettingsCommandHandler : IRequestHandler<UpsertBranchRequestSettingsCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public UpsertBranchRequestSettingsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        UpsertBranchRequestSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .Include(b => b.RequestSettings)
            .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

        if (branch == null)
            return Error.NotFound(description: "Branch not found.");

        if (request.Settings.Count == 0)
            return Error.Validation(description: "Please provide at least one request setting.");

        var settingsByType = branch.RequestSettings.ToDictionary(s => s.RequestType);

        foreach (var payload in request.Settings)
        {
            if (settingsByType.TryGetValue(payload.RequestType, out var entity))
            {
                entity.IsVisibleToEmployees = payload.IsVisibleToEmployees;
                entity.AllowEmployeesToSubmit = payload.AllowEmployeesToSubmit;
                entity.RequireAttachment = payload.RequireAttachment;
                entity.MaxOpenRequests = payload.MaxOpenRequests;
                entity.CustomInstructions = payload.CustomInstructions;
            }
            else
            {
                var newSetting = new BranchRequestSetting
                {
                    RequestType = payload.RequestType,
                    IsVisibleToEmployees = payload.IsVisibleToEmployees,
                    AllowEmployeesToSubmit = payload.AllowEmployeesToSubmit,
                    RequireAttachment = payload.RequireAttachment,
                    MaxOpenRequests = payload.MaxOpenRequests,
                    CustomInstructions = payload.CustomInstructions,
                    BranchId = branch.Id,
                    TenantId = branch.TenantId,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                branch.RequestSettings.Add(newSetting);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Branch request settings updated successfully");
    }
}
