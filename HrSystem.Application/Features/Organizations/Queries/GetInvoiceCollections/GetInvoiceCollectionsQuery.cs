using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetInvoiceCollections;

public record GetInvoiceCollectionsQuery(bool? IsCollected)
    : IRequest<ErrorOr<GenericResponse<List<InvoiceCollectionDto>>>>;

public record InvoiceCollectionDto
{
    public Guid InvoiceId { get; init; }
    public Guid OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? PaidDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public string Currency { get; init; } = "EGP";
    public string Status { get; init; } = string.Empty;
    public bool IsCollected { get; init; }
}

public class GetInvoiceCollectionsQueryHandler
    : IRequestHandler<GetInvoiceCollectionsQuery, ErrorOr<GenericResponse<List<InvoiceCollectionDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetInvoiceCollectionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<InvoiceCollectionDto>>>> Handle(
        GetInvoiceCollectionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OrganizationInvoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(i => i.Organization)
            .Include(i => i.Status)
            .Where(i => !i.IsDeleted);

        if (request.IsCollected.HasValue)
        {
            if (request.IsCollected.Value)
            {
                query = query.Where(i =>
                    i.Status.Code == "PAID" ||
                    i.PaidDate.HasValue ||
                    i.RemainingAmount <= 0m);
            }
            else
            {
                query = query.Where(i =>
                    i.Status.Code != "PAID" &&
                    !i.PaidDate.HasValue &&
                    i.RemainingAmount > 0m);
            }
        }

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.CreatedDate)
            .Select(i => new InvoiceCollectionDto
            {
                InvoiceId = i.Id,
                OrganizationId = i.OrganizationId,
                OrganizationName = string.IsNullOrWhiteSpace(i.Organization.NameEn)
                    ? i.Organization.NameAr
                    : i.Organization.NameEn,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                PaidDate = i.PaidDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                Currency = string.IsNullOrWhiteSpace(i.Organization.Currency) ? "EGP" : i.Organization.Currency!,
                Status = string.IsNullOrWhiteSpace(i.Status.NameEn) ? i.Status.Code : i.Status.NameEn,
                IsCollected =
                    i.Status.Code == "PAID" ||
                    i.PaidDate.HasValue ||
                    i.RemainingAmount <= 0m
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<InvoiceCollectionDto>>.SuccessResult(
            invoices,
            "Invoice collection list retrieved successfully.");
    }
}
