using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetSuperAdminInvoices;

public record GetSuperAdminInvoicesQuery
    : IRequest<ErrorOr<GenericResponse<List<SuperAdminInvoiceDto>>>>;

public record SuperAdminInvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Amount { get; init; }
}

public record SuperAdminInvoiceDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime PeriodStartDate { get; init; }
    public DateTime PeriodEndDate { get; init; }
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Currency { get; init; } = "EGP";
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<SuperAdminInvoiceItemDto> Items { get; init; } = new();
}

public class GetSuperAdminInvoicesQueryHandler
    : IRequestHandler<GetSuperAdminInvoicesQuery, ErrorOr<GenericResponse<List<SuperAdminInvoiceDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetSuperAdminInvoicesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<SuperAdminInvoiceDto>>>> Handle(
        GetSuperAdminInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await _context.OrganizationInvoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(i => i.Organization)
            .Include(i => i.Status)
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.CreatedDate)
            .Select(i => new SuperAdminInvoiceDto
            {
                Id = i.Id,
                OrganizationId = i.OrganizationId,
                OrganizationName = string.IsNullOrWhiteSpace(i.Organization.NameEn)
                    ? i.Organization.NameAr
                    : i.Organization.NameEn,
                InvoiceNumber = i.InvoiceNumber,
                PeriodStartDate = i.PeriodStartDate,
                PeriodEndDate = i.PeriodEndDate,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Currency = string.IsNullOrWhiteSpace(i.Organization.Currency) ? "EGP" : i.Organization.Currency!,
                SubTotal = i.SubTotal,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                Status = string.IsNullOrWhiteSpace(i.Status.NameEn) ? i.Status.Code : i.Status.NameEn,
                Notes = i.Notes,
                Items = i.Items
                    .OrderBy(x => x.CreatedDate)
                    .Select(x => new SuperAdminInvoiceItemDto
                    {
                        Description = string.IsNullOrWhiteSpace(x.DescriptionEn) ? x.DescriptionAr : x.DescriptionEn,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Amount = x.Amount
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<SuperAdminInvoiceDto>>.SuccessResult(
            invoices,
            "SuperAdmin invoices retrieved successfully.");
    }
}
