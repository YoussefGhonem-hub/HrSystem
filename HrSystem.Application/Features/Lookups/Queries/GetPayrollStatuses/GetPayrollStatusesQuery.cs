using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetPayrollStatuses;

/// <summary>
/// Query to get all active payroll statuses for dropdown
/// </summary>
public record GetPayrollStatusesQuery : IRequest<ErrorOr<GenericResponse<List<PayrollStatusDto>>>>;

public class GetPayrollStatusesQueryHandler : IRequestHandler<GetPayrollStatusesQuery, ErrorOr<GenericResponse<List<PayrollStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<PayrollStatusDto>>>> Handle(
        GetPayrollStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.PayrollStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PayrollStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                Description = s.Description,
                ColorCode = s.ColorCode,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<PayrollStatusDto>>
        {
            Success = true,
            Message = "Payroll statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class PayrollStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
}
