using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyPaymentDetails;

public record GetMyPaymentDetailsQuery(int? Year = null, int? Month = null) : IRequest<ErrorOr<GenericResponse<MyPaymentDetailsDto>>>;

public class GetMyPaymentDetailsQueryHandler : IRequestHandler<GetMyPaymentDetailsQuery, ErrorOr<GenericResponse<MyPaymentDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyPaymentDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyPaymentDetailsDto>>> Handle(GetMyPaymentDetailsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;

        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Payroll.Unauthorized", "Current user is not linked to an employee");
        }

        var employee = await _context.Employees
            .Where(e => e.Id == employeeId.Value)
            .Select(e => new { e.FullNameEn, e.FullNameAr })
            .FirstOrDefaultAsync(cancellationToken);

        var salary = await _context.Salaries
            .Where(s => s.EmployeeId == employeeId.Value && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (salary == null)
        {
            return Error.NotFound("Payroll.NoSalary", "No salary configuration found for the current user");
        }

        var cycle = await _context.PayrollCycles
            .Where(c => c.Year == year && c.Month == month)
            .OrderByDescending(c => c.PaymentDate)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new MyPaymentDetailsDto
        {
            PaymentMethod = salary.PaymentMethod,
            BankName = salary.BankName,
            IbanMasked = MaskIban(salary.BankIban),
            AccountHolderName = !string.IsNullOrWhiteSpace(employee?.FullNameEn) ? employee!.FullNameEn : employee?.FullNameAr,
            SalaryPaymentDay = FormatPaymentDay(cycle?.PaymentDate)
        };

        return new GenericResponse<MyPaymentDetailsDto>
        {
            Success = true,
            Message = "Payment details retrieved successfully",
            Data = dto
        };
    }

    private static string? MaskIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return null;

        var raw = new string(iban.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        if (raw.Length <= 8) return iban; // too short to mask properly, return as-is

        var first = raw.Substring(0, 4);
        var last = raw.Substring(raw.Length - 4);

        // Build groups of 4 with masked middle
        var groups = new List<string>();
        groups.Add(first);
        var middleLen = raw.Length - 8;
        var middleGroups = (int)Math.Ceiling(middleLen / 4.0);
        for (int i = 0; i < middleGroups; i++) groups.Add("****");
        groups.Add(last);
        return string.Join(" ", groups);
    }

    private static string? FormatPaymentDay(DateTime? paymentDate)
    {
        if (!paymentDate.HasValue) return null;
        var day = paymentDate.Value.Day;
        var suffix = GetDaySuffix(day);
        return $"{day}{suffix} of every month"; // UI-friendly text
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
