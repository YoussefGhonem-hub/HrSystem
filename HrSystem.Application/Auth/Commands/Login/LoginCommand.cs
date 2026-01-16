using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<ErrorOr<GenericResponse<LoginResponse>>>;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string FullName,
    string Email,
    List<string> Roles);
