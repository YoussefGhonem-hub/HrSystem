using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetEmployeePaymentDetails;

public record GetEmployeePaymentDetailsQuery(Guid EmployeeId, int? Year = null, int? Month = null)
    : IRequest<ErrorOr<GenericResponse<EmployeePaymentDetailsDto>>>;

public class GetEmployeePaymentDetailsQueryHandler
    : IRequestHandler<GetEmployeePaymentDetailsQuery, ErrorOr<GenericResponse<EmployeePaymentDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeePaymentDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeePaymentDetailsDto>>> Handle(
        GetEmployeePaymentDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;

        var employee = await _context.Employees
            .Where(e => e.Id == request.EmployeeId)
            .Select(e => new
            {
                e.Id,
                e.FullNameEn,
                e.FullNameAr,
                e.EmployeeCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Payroll.EmployeeNotFound", "Employee not found");
        }

        var salary = await _context.Salaries
            .Where(s => s.EmployeeId == request.EmployeeId && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (salary is null)
        {
            return Error.NotFound("Payroll.NoSalary", "No salary configuration found for this employee");
        }

        var cycle = await _context.PayrollCycles
            .Where(c => c.Year == year && c.Month == month)
            .OrderByDescending(c => c.PaymentDate)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new EmployeePaymentDetailsDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = !string.IsNullOrWhiteSpace(employee.FullNameEn)
                ? employee.FullNameEn
                : employee.FullNameAr,
            PaymentMethod = salary.PaymentMethod,
            BankName = salary.BankName,
            IbanMasked = MaskIban(salary.BankIban),
            AccountHolderName = !string.IsNullOrWhiteSpace(employee.FullNameEn)
                ? employee.FullNameEn
                : employee.FullNameAr,
            SalaryPaymentDay = FormatPaymentDay(cycle?.PaymentDate)
        };

        return new GenericResponse<EmployeePaymentDetailsDto>
        {
            Success = true,
            Message = "Transfer method details retrieved successfully",
            Data = dto
        };
    }

    private static string? MaskIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return null;

        var raw = new string(iban.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        if (raw.Length <= 8) return iban;

        var first = raw[..4];
        var last = raw[^4..];

        var groups = new List<string> { first };
        var middleLen = raw.Length - 8;
        var middleGroups = (int)Math.Ceiling(middleLen / 4.0);
        for (var i = 0; i < middleGroups; i++) groups.Add("****");
        groups.Add(last);

        return string.Join(" ", groups);
    }

    private static string? FormatPaymentDay(DateTime? paymentDate)
    {
        if (!paymentDate.HasValue) return null;
        var day = paymentDate.Value.Day;
        var suffix = GetDaySuffix(day);
        return $"{day}{suffix} of every month";
    }

    private static string GetDaySuffix(int day)
    {
        if (day % 100 is 11 or 12 or 13) return "th";
        return (day % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}
