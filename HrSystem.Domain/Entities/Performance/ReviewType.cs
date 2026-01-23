using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

public class ReviewType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public virtual ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
}
