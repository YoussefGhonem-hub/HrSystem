using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<ErrorOr<GenericResponse<LoginResponse>>>;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string FullName,
    string Email,
    List<string> Roles,
    Guid? BranchId,
    Guid? EmployeeId,
    List<BranchRequestAvailabilityDto> BranchRequestAccess);
