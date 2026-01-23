using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetInvoiceStatuses;

public record GetInvoiceStatusesQuery : IRequest<ErrorOr<GenericResponse<List<InvoiceStatusDto>>>>;

public class GetInvoiceStatusesQueryHandler : IRequestHandler<GetInvoiceStatusesQuery, ErrorOr<GenericResponse<List<InvoiceStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetInvoiceStatusesQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<InvoiceStatusDto>>>> Handle(
        GetInvoiceStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.InvoiceStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new InvoiceStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                DescriptionEn = s.DescriptionEn,
                DescriptionAr = s.DescriptionAr,
                ColorCode = s.ColorCode,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<InvoiceStatusDto>>
        {
            Success = true,
            Data = statuses
        };
    }
}
