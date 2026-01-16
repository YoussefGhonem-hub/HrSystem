using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    string? City,
    string? AddressAr,
    string? AddressEn,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? PhoneNumber,
    string? Email,
    string? Fax,
    string TimeZone,
    string Currency,
    string? Language,
    bool IsActive,
    Guid? BranchManagerId
) : IRequest<ErrorOr<GenericResponse<BranchDto>>>;
