using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.CollectOrganizationInvoice;

public record CollectOrganizationInvoiceCommand(
    Guid InvoiceId,
    string? PaymentMethod,
    string? PaymentReference,
    DateTime? PaidDate,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<CollectOrganizationInvoiceDto>>>;

public record CollectOrganizationInvoiceDto
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public DateTime PaidDate { get; init; }
    public string Status { get; init; } = "Paid";
}

public class CollectOrganizationInvoiceCommandHandler
    : IRequestHandler<CollectOrganizationInvoiceCommand, ErrorOr<GenericResponse<CollectOrganizationInvoiceDto>>>
{
    private readonly ApplicationDbContext _context;

    public CollectOrganizationInvoiceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<CollectOrganizationInvoiceDto>>> Handle(
        CollectOrganizationInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _context.OrganizationInvoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && !i.IsDeleted, cancellationToken);

        if (invoice is null)
        {
            return Error.NotFound(
                code: "Organization.Invoice.NotFound",
                description: "Invoice not found.");
        }

        var currentStatusCode = await _context.InvoiceStatuses
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted && s.Id == invoice.StatusId)
            .Select(s => s.Code)
            .FirstOrDefaultAsync(cancellationToken);

        var isAlreadyCollected =
            string.Equals(currentStatusCode, "PAID", StringComparison.OrdinalIgnoreCase) ||
            invoice.PaidDate.HasValue ||
            invoice.RemainingAmount <= 0m;

        if (isAlreadyCollected)
        {
            return Error.Validation(
                code: "Organization.Invoice.AlreadyCollected",
                description: "Invoice is already marked as collected.");
        }

        var paidDateUtc = request.PaidDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var paidStatusId = await _context.InvoiceStatuses
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => s.Code == "PAID" || s.NameEn == "Paid")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!paidStatusId.HasValue)
        {
            return Error.NotFound(
                code: "Organization.InvoiceStatus.PaidNotFound",
                description: "Paid invoice status is missing. Please add an active invoice status with code PAID.");
        }

        invoice.PaidAmount = invoice.TotalAmount;
        invoice.RemainingAmount = 0m;
        invoice.PaidDate = paidDateUtc;
        invoice.StatusId = paidStatusId.Value;

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            invoice.PaymentMethod = request.PaymentMethod.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentReference))
        {
            invoice.PaymentReference = request.PaymentReference.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            invoice.Notes = request.Notes.Trim();
        }

        invoice.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CollectOrganizationInvoiceDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            RemainingAmount = invoice.RemainingAmount,
            PaidDate = invoice.PaidDate!.Value,
            Status = "Paid"
        };

        return GenericResponse<CollectOrganizationInvoiceDto>.SuccessResult(
            dto,
            "Invoice marked as collected successfully.");
    }
}
