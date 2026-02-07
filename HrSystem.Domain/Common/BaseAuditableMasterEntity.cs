using System.ComponentModel.DataAnnotations;

namespace HrSystem.Domain.Common;

public class BaseAuditableMasterEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }

    public BaseAuditableMasterEntity()
    {
        Id = Guid.NewGuid();
    }

    public void MarkAsCreated(Guid currentUserId)
    {
        CreatedBy = currentUserId;
        CreatedDate = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    public void MarkAsNotDeleted()
    {
        IsDeleted = false;
        DeletedDate = null;
        DeletedBy = null;
    }

    public void MarkAsDeleted(Guid currentUserId)
    {
        IsDeleted = true;
        DeletedDate = DateTimeOffset.UtcNow;
        DeletedBy = currentUserId;
    }

    public void MarkAsModified(Guid currentUserId)
    {
        ModifiedBy = currentUserId;
        ModifiedDate = DateTimeOffset.UtcNow;
    }
}
