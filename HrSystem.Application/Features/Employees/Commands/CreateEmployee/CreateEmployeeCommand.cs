using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstNameAr,
    string LastNameAr,
    string FirstNameEn,
    string LastNameEn,
    string NationalId,
    string? PassportNumber,
    DateTime DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
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
    ContractType ContractType,
    DateTime HiringDate,
    int ProbationPeriodMonths
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;
