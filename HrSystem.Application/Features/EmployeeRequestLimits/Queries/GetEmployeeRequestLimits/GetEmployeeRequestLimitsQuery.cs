using System;
using System.Linq;
using ErrorOr;
using HrSystem.Application.Features.EmployeeRequestLimits;
using HrSystem.Application.Features.EmployeeRequestLimits.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequestLimits.Queries.GetEmployeeRequestLimits;

public record GetEmployeeRequestLimitsQuery(Guid EmployeeId)
    : IRequest<ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>>;

public class GetEmployeeRequestLimitsQueryHandler
    : IRequestHandler<GetEmployeeRequestLimitsQuery, ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeRequestLimitsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestLimitsDto>>> Handle(
        GetEmployeeRequestLimitsQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == request.EmployeeId)
            .Select(e => new EmployeeSnapshot(
                e.Id,
                e.EmployeeCode,
                (e.FirstNameEn + " " + e.LastNameEn).Trim(),
                (e.FirstNameAr + " " + e.LastNameAr).Trim()))
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found.");
        }

        var employeeName = !string.IsNullOrWhiteSpace(employee.EnglishName)
            ? employee.EnglishName
            : !string.IsNullOrWhiteSpace(employee.ArabicName)
                ? employee.ArabicName
                : employee.EmployeeCode;

        var response = await EmployeeRequestLimitResponseBuilder.BuildAsync(
            _context,
            employee.Id,
            employee.EmployeeCode,
            employeeName,
            cancellationToken);

        return GenericResponse<EmployeeRequestLimitsDto>.SuccessResult(
            response,
            "Employee request limits retrieved successfully.");
    }

    private sealed record EmployeeSnapshot(
        Guid Id,
        string EmployeeCode,
        string EnglishName,
        string ArabicName);
}
