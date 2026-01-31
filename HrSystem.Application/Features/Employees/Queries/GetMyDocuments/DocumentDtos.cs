using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.Queries.GetMyDocuments;

public class DocumentGroupDto
{
    public DocumentCategory CategoryKey { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;
    public List<DocumentItemDto> Items { get; set; } = new();
}

public class DocumentItemDto
{
    public Guid Id { get; set; }
    public EmployeeDocumentType DocumentType { get; set; }
    public string DocumentTypeNameEn { get; set; } = string.Empty;
    public string DocumentTypeNameAr { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
    public DocumentCategory Category { get; set; }
}
