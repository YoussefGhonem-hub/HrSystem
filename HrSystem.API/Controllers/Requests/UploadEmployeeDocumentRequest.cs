using Microsoft.AspNetCore.Http;

namespace HrSystem.API.Controllers.Requests;

public class UploadEmployeeDocumentRequest
{
    public Guid DocumentTypeId { get; set; }
    public string? DocumentName { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public IFormFile File { get; set; } = null!;
}
