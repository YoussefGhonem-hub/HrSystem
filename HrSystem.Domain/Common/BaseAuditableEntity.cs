namespace HrSystem.Domain.Common;

public class BaseAuditableEntity : BaseEntity
{
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