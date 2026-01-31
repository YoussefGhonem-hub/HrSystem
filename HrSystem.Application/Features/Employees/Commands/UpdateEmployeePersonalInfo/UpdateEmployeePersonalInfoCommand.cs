using System;
using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePersonalInfo;

public record UpdateEmployeePersonalInfoCommand(
    Guid EmployeeId,
    string FirstNameAr,
    string LastNameAr,
    string FirstNameEn,
    string LastNameEn,
    string NationalId,
    string? PassportNumber,
    DateTime DateOfBirth,
    Guid GenderId,
    Guid MaritalStatusId,
    string Email,
    string PhoneNumber,
    string? MobileNumber,
    string AddressAr,
    string? AddressEn,
    string? City,
    string? Country
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
