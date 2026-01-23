using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    string FirstNameAr,
    string LastNameAr,
    string FirstNameEn,
    string LastNameEn,
    string? PassportNumber,
    Guid MaritalStatusId,
    string Email,
    string PhoneNumber,
    string? MobileNumber,
    string AddressAr,
    string? AddressEn,
    string? City,
    string? Country,
    Guid DepartmentId,
    Guid JobTitleId,
    Guid? DirectManagerId,
    Guid? BranchId,
    Guid ContractTypeId,
    Guid StatusId
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
